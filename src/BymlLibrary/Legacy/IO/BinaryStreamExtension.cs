using Syroot.BinaryData;

namespace BymlLibrary.Legacy.IO;

/// <summary>
/// BinaryStream extension methods
/// </summary>
public static class BinaryStreamExtension
{
    internal static void SatisfyOffset(this BinaryStream stream, uint offset, uint value)
    {
        using (stream.TemporarySeek(offset, SeekOrigin.Begin))
            stream.Write(value);
    }

    internal static uint ReserveOffset(this BinaryStream stream)
    {
        var pos = (uint)stream.Position;
        stream.Position += 4L;
        return pos;
    }

    internal static NodeType PeekNodeType(this BinaryStream stream)
    {
        using (stream.TemporarySeek()) {
            // If the offset is invalid, the type cannot be determined.
            uint offset = stream.ReadUInt32();
            if (offset > 0 && offset < stream.BaseStream.Length) {
                // Seek to the offset and try to read a valid type.
                stream.Position = offset;
                NodeType nodeType = (NodeType)stream.ReadByte();
                if (Enum.IsDefined(typeof(NodeType), nodeType))
                    return nodeType;
            }
        }
        return NodeType.None;
    }

    internal static uint ReadUInt24(this BinaryStream stream)
    {
        List<byte> list = stream.ReadBytes(3).ToList();
        if (stream.ByteConverter.Endian == Syroot.BinaryData.Core.Endian.Big) {
            list.Reverse();
        }
        list.Add(0);
        return BitConverter.ToUInt32(list.ToArray());
    }

    internal static void WriteUInt24(this BinaryStream stream, uint i)
    {
        byte[] bytes = BitConverter.GetBytes(i)[0..^1];
        if (stream.ByteConverter.Endian == Syroot.BinaryData.Core.Endian.Big) {
            List<byte> temp = bytes.ToList();
            temp.Reverse();
            bytes = temp.ToArray();
        }
        stream.Write(bytes);
    }

    internal static void WriteAt(this BinaryStream stream, uint location, uint data)
    {
        using (stream.TemporarySeek(location, SeekOrigin.Begin)) {
            stream.WriteUInt32(data);
        }
    }
}
