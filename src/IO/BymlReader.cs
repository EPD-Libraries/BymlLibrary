using Nintendo.Byml.Collections;
using Syroot.BinaryData;
using Syroot.BinaryData.Core;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nintendo.Byml.IO
{
    internal class BymlReader : BymlFile
    {
        private BymlNode NameArray { get; set; }
        private BymlNode StringArray { get; set; }

        public BymlFile Read(Stream stream, Encoding encoding)
        {
            // Open a reader on the given stream.
            using (BinaryStream reader = new(stream, encoding: encoding, leaveOpen: true))
            {
                ushort magic = reader.ReadUInt16();
                
                if (magic == 0x5942)
                    Endianness = Endian.Big;
                else if (magic == 0x4259)
                    Endianness = Endian.Little;
                else
                    throw new BymlException($"Could not decompile BYML. Invalid header '{magic}'.");

                reader.ByteConverter = ByteConverter.GetConverter(Endianness);

                // Get BYML version
                Version = reader.ReadUInt16();
                if (Version < 2 || Version > 4)
                    throw new BymlException($"Unexpected version {Version}");

                // Read the name array, holding strings referenced by index for the names of other nodes.
                uint offset = reader.ReadUInt32();
                if (offset > 0)
                {
                    NameArray = ReadEnumerableNode(reader, offset);
                }
                offset = reader.ReadUInt32();
                if (offset > 0)
                {
                    StringArray = ReadEnumerableNode(reader, offset);
                }

                if (reader.BaseStream.Length <= 16)
                {
                    RootNode = new BymlNode(new List<BymlNode>());
                }

                offset = reader.ReadUInt32();
                if (offset == 0)
                {
                    RootNode = new BymlNode(new List<BymlNode>());
                }
                else
                {
                    RootNode = ReadEnumerableNode(reader, offset);
                }
            }

            stream.Dispose();
            return this;
        }

        private uint Get1MsbByte(uint value) => Endianness == Endian.Big ? value & 0x000000FF : value >> 24;
        private uint Get3LsbBytes(uint value) => Endianness == Endian.Big ? value & 0x00FFFFFF : value >> 8;
        private uint Get3MsbBytes(uint value) => Endianness == Endian.Little ? value & 0x00FFFFFF : value >> 8;

        private BymlNode ReadEnumerableNode(BinaryStream reader, uint offset)
        {
            BymlNode node;
            using (reader.TemporarySeek(offset, SeekOrigin.Begin))
            {
                NodeType type = (NodeType)reader.ReadByte();
                reader.Seek(-1);
                int length = (int)Get3LsbBytes(reader.ReadUInt32());
                node = type switch
                {
                    NodeType.Array => ReadArrayNode(reader, length),
                    NodeType.Hash => ReadDictionaryNode(reader, length),
                    NodeType.StringArray => ReadStringArrayNode(reader, length),
                    _ => throw new BymlException($"Invalid enumerable node type {type}."),
                };
            }
            return node;
        }

        public BymlNode ReadArrayNode(BinaryStream reader, int length)
        {
            BymlNode node = new BymlNode(new List<BymlNode>());
            IEnumerable<NodeType> types = reader.ReadBytes(length).Select(x => (NodeType)x);
            reader.Align(4);

            foreach (NodeType type in types)
            {
                BymlNode value = ReadNode(reader, type);
                if (type.IsEnumerable())
                    node.Array.Add(ReadEnumerableNode(reader, value.UInt));
                else
                    node.Array.Add(value);
            }

            return node;
        }

        private BymlNode ReadDictionaryNode(BinaryStream reader, int length)
        {
            BymlNode node = new(new Dictionary<string, BymlNode>());
            SortedList<uint, string> enumerables = new(new DuplicateKeyComparer<uint>());
            // Read the elements of the dictionary.
            for (int i = 0; i < length; i++)
            {
                uint indexAndType = reader.ReadUInt32();
                int nodeNameIndex = (int)Get3MsbBytes(indexAndType);
                NodeType type = (NodeType)Get1MsbByte(indexAndType);

                string name = NameArray.Array[nodeNameIndex].String;
                BymlNode value = ReadNode(reader, type);

                if (type.IsEnumerable())
                    node.Hash.Add(name, ReadEnumerableNode(reader, value.UInt));
                else
                    node.Hash.Add(name, value);
            }

            // Read the offset enumerable nodes in the order of how they appear in the file.
            foreach ((uint key, string value) in enumerables)
                node.Hash.Add(value, ReadEnumerableNode(reader, key));

            return node;
        }

        private static BymlNode ReadStringArrayNode(BinaryStream reader, int length)
        {
            // Element offsets are relative to the start of node.
            long nodeOffset = reader.Position - sizeof(uint);
            BymlNode node = new(new List<BymlNode>());

            // Read the element offsets.
            uint[] offsets = reader.ReadUInt32s(length);

            // Read the elements.
            foreach (uint offset in offsets)
            {
                reader.Position = nodeOffset + offset;
                node.Array.Add(new(reader.ReadString(StringCoding.ZeroTerminated)));
            }

            return node;
        }

        private BymlNode ReadNode(BinaryStream reader, NodeType nodeType)
        {
            // Read the following UInt32 which is representing the value directly.
            return nodeType switch
            {
                NodeType.Array or NodeType.Hash or NodeType.StringArray => new BymlNode(reader.ReadUInt32()),// offset
                NodeType.String => new BymlNode(StringArray.Array[reader.ReadInt32()].String),
                NodeType.Bool => new BymlNode(reader.ReadInt32() != 0),
                NodeType.Int => new BymlNode(reader.ReadInt32()),
                NodeType.Float => new BymlNode(reader.ReadSingle()),
                NodeType.UInt => new BymlNode(reader.ReadUInt32()),
                NodeType.Int64 or NodeType.UInt64 or NodeType.Double => ReadNodeFromOffset(reader, nodeType),
                NodeType.Null => new BymlNode(),
                _ => throw new BymlException($"Unknown node type '{nodeType:X}'."),
            };
        }

        private static BymlNode ReadNodeFromOffset(BinaryStream reader, NodeType nodeType)
        {
            // Set position
            var pos = reader.Position;
            reader.Position = reader.ReadUInt32();

            // Get node value
            dynamic value = nodeType switch
            {
                NodeType.Int64 => reader.ReadInt64(),
                NodeType.UInt64 => reader.ReadUInt64(),
                NodeType.Double => reader.ReadDouble(),
                _ => throw new BymlException($"Unknown node type '{nodeType}'."),
            };

            // Reset position
            reader.Position = pos + 4;

            return value;
        }
    }
}
