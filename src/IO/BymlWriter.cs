using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Syroot.BinaryData;

namespace Nintendo.Byml.IO
{
    internal class BymlWriter
    {
        public static byte[] Write(BymlFile byml, Encoding encoding)
        {
            byte[] bytes;
            using (BinaryStream stream = new(new MemoryStream(), ByteConverter.GetConverter(byml.Endianness), encoding))
            {
                if (byml.Version < 2 || byml.Version > 4)
                    throw new InvalidDataException($"Invalid BYML version {byml.Version}");

                WriteContext context = new(byml.RootNode) { Writer = stream };

                context.Writer.WriteUInt16(0x4259);
                context.Writer.WriteUInt16(byml.Version);
                context.Writer.WriteUInt32(0); // Hash key table offset
                context.Writer.WriteUInt32(0); // String table offset
                context.Writer.WriteUInt32(0); // Root node offset

                if (byml.RootNode.Type == NodeType.Null)
                {
                    return ((MemoryStream)context.Writer.BaseStream).ToArray();
                }

                if (!context.HashKeys.IsEmpty())
                {
                    context.Writer.WriteAt(4u, (uint)context.Writer.Position);
                    context.WriteStringTable(context.HashKeys);
                }

                if (!context.Strings.IsEmpty())
                {
                    context.Writer.WriteAt(8u, (uint)context.Writer.Position);
                    context.WriteStringTable(context.Strings);
                }

                context.Writer.WriteAt(12u, (uint)context.Writer.Position);
                context.Writer.Align(4);
                context.WriteContainerNode(byml.RootNode);
                context.Writer.Align(4);

                bytes = ((MemoryStream)context.Writer.BaseStream).ToArray();
            }
            return bytes;
        }
    }

    internal class WriteContext
    {
        private int num_non_inline_nodes = 0;
        private StringTable hash_key_table;
        private StringTable string_table;
        private Dictionary<BymlNode, uint> non_inline_node_data = new();
        public BinaryStream Writer { get; set; }
        public StringTable HashKeys { get => hash_key_table; }
        public StringTable Strings { get => string_table; }

        public WriteContext(BymlNode root)
        {
            hash_key_table = new();
            string_table = new();

            traverse(root);

            hash_key_table.Build();
            string_table.Build();

            void traverse(BymlNode node)
            {
                NodeType type = node.Type;
                if (IsNonInlineType(type))
                    num_non_inline_nodes++;
                switch (type)
                {
                    case NodeType.String:
                        string_table.Add(node.String);
                        break;
                    case NodeType.Array:
                        foreach (BymlNode child in node.Array)
                            traverse(child);
                        break;
                    case NodeType.Hash:
                        foreach ((string key, BymlNode child) in node.Hash)
                        {
                            hash_key_table.Add(key);
                            traverse(child);
                        }
                        break;
                }
            }
        }

        public static bool IsNonInlineType(NodeType type)
        {
            return IsContainerType(type) || IsLongType(type) || type == NodeType.Binary;
        }

        public static bool IsContainerType(NodeType type)
        {
            return type == NodeType.Array || type == NodeType.Hash;
        }

        public static bool IsLongType(NodeType type)
        {
            return type == NodeType.Int64 || type == NodeType.UInt64 || type == NodeType.Double;
        }

        public void WriteValueNode(BymlNode node)
        {
            switch (node.Type)
            {
                case NodeType.Null:
                    Writer.WriteUInt32(0);
                    break;
                case NodeType.String:
                    Writer.Write(string_table.GetIndex(node.String));
                    break;
                case NodeType.Binary:
                    Writer.WriteUInt32((uint)node.Binary.Length);
                    Writer.WriteBytes(node.Binary);
                    break;
                case NodeType.Bool:
                    Writer.WriteUInt32(node.Bool ? 1u : 0u);
                    break;
                case NodeType.Int:
                    Writer.WriteInt32(node.Int);
                    break;
                case NodeType.Float:
                    Writer.WriteSingle(node.Float);
                    break;
                case NodeType.UInt:
                    Writer.WriteUInt32(node.UInt);
                    break;
                case NodeType.Int64:
                    Writer.WriteInt64(node.Int64);
                    break;
                case NodeType.UInt64:
                    Writer.WriteUInt64(node.UInt64);
                    break;
                case NodeType.Double:
                    Writer.WriteDouble(node.Double);
                    break;
                default:
                    throw new InvalidDataException($"{node.Type} is not a value node!");
            }
        }

        private class NonInlineNode
        {
            public uint ContainerOffset { get; set; }
            public BymlNode Node { get; set; }

            public NonInlineNode(uint containerOffset, BymlNode node)
            {
                ContainerOffset = containerOffset;
                Node = node;
            }
        }

        public void WriteContainerNode(BymlNode node)
        {
            List<NonInlineNode> non_inline_nodes = new();

            switch (node.Type)
            {
                case NodeType.Array:
                    Writer.Write((byte)NodeType.Array);
                    Writer.WriteUInt24((uint)node.Array.Count);
                    foreach (BymlNode item in node.Array)
                        Writer.WriteByte((byte)item.Type);
                    Writer.Align(4);
                    foreach (BymlNode item in node.Array)
                        write_container_item(item);
                    break;
                case NodeType.Hash:
                    SortedDictionary<string, BymlNode> hash = node.Hash;
                    Writer.WriteByte((byte)node.Type);
                    Writer.WriteUInt24((uint)hash.Count);
                    foreach ((string key, BymlNode child) in hash)
                    {
                        Writer.WriteUInt24(hash_key_table.GetIndex(key));
                        Writer.WriteByte((byte)child.Type);
                        write_container_item(child);
                    }
                    break;
                default:
                    throw new ArgumentException($"Invalid container node type {node.Type}");
            }

            foreach (NonInlineNode non in non_inline_nodes)
            {
                if (non_inline_node_data.TryGetValue(non.Node, out uint offset))
                {
                    using (Writer.TemporarySeek(non.ContainerOffset, SeekOrigin.Begin))
                    {
                        Writer.WriteUInt32(offset);
                    }
                }
                else
                {
                    offset = (uint)Writer.Position;
                    using (Writer.TemporarySeek(non.ContainerOffset, SeekOrigin.Begin))
                    {
                        Writer.WriteUInt32(offset);
                    }
                    non_inline_node_data.Add(non.Node, offset);
                    if (IsContainerType(non.Node.Type))
                    {
                        WriteContainerNode(non.Node);
                    }
                    else
                    {
                        WriteValueNode(non.Node);
                    }
                }
            }

            void write_container_item(BymlNode node)
            {
                if (IsNonInlineType(node.Type))
                {
                    non_inline_nodes.Add(new NonInlineNode((uint)Writer.Position, node));
                    Writer.WriteUInt32(0);
                }
                else
                {
                    WriteValueNode(node);
                }
            }
        }

        public void WriteStringTable(StringTable table)
        {
            uint start = (uint)Writer.Position;
            Writer.WriteByte((byte)NodeType.StringArray);
            Writer.WriteUInt24(table.Size);

            // String offsets
            uint offset_table_offset = (uint)Writer.Position;
            Writer.Seek(sizeof(uint) * (table.Size + 1));
            
            int i = 0;
            foreach (string s in table.Strings)
            {
                Writer.WriteAt((uint)(offset_table_offset + sizeof(uint) * i), (uint)Writer.Position - start);
                Writer.WriteBytes(Writer.Encoding.GetBytes($"{s}\0"));
                i++;
            }

            Writer.WriteAt((uint)(offset_table_offset + sizeof(uint) * i), (uint)Writer.Position - start);
            Writer.Align(4);
        }
    }

    internal class StringTable
    {
        private bool built;
        private readonly Dictionary<string, uint> hash_table = new();
        private readonly SortedSet<string> sorted_strings = new(new AsciiComparer());
        public uint Size { get => (uint)sorted_strings.Count; }
        public SortedSet<string> Strings { get => sorted_strings; }
        public bool IsEmpty() => sorted_strings.Count == 0;
        public void Add(string str)
        {
            if (built)
                throw new InvalidOperationException("Can't add strings after the table has been built");
            sorted_strings.Add(str);
        }
        public string GetString(uint index)
        {
            if (!built)
                throw new InvalidOperationException("Table hasn't been built yet, strings are in the wrong order");
            return sorted_strings.ToList()[(int)index];
        }
        public uint GetIndex(string str)
        {
            if (!built)
                throw new InvalidOperationException("Table hasn't been built yet, strings are in the wrong order");
            return hash_table[str];
        }

        public void Build()
        {
            uint count = 0;
            foreach (string str in sorted_strings)
            {
                hash_table[str] = count++;
            }
            built = true;
        }
    }
}
