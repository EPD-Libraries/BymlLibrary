using BymlLibrary.Nodes.Containers;
using BymlLibrary.Nodes.Immutable.Containers;
using System.Buffers;
using System.Text;
using VYaml.Emitter;

namespace BymlLibrary.Yaml;

public static class BymlYamlWriter
{
    public static void Write(ref Utf8YamlEmitter emitter, in ImmutableByml byml, in ImmutableByml root)
    {
        byte[] formattedFloatRentedBuffer = ArrayPool<byte>.Shared.Rent(12);
        Span<byte> formattedFloatBuffer = formattedFloatRentedBuffer.AsSpan()[..12];

        switch (byml.Type) {
            case BymlNodeType.HashMap32:
                byml.GetHashMap32().EmitYaml(ref emitter, root);
                break;
            case BymlNodeType.HashMap64:
                byml.GetHashMap64().EmitYaml(ref emitter, root);
                break;
            case BymlNodeType.String:
                WriteRawString(ref emitter, byml.GetStringIndex(), root.StringTable);
                break;
            case BymlNodeType.Binary:
                Span<byte> data = byml.GetBinary();
                WriteBinary(ref emitter, data);
                break;
            case BymlNodeType.BinaryAligned:
                Span<byte> dataAligned = byml.GetBinaryAligned(out int alignment);
                WriteBinaryAligned(ref emitter, dataAligned, alignment);
                break;
            case BymlNodeType.Array:
                byml.GetArray().EmitYaml(ref emitter, root);
                break;
            case BymlNodeType.Map:
                byml.GetMap().EmitYaml(ref emitter, root);
                break;
            case BymlNodeType.Bool:
                emitter.WriteBool(byml.GetBool());
                break;
            case BymlNodeType.Int:
                emitter.WriteInt32(byml.GetInt());
                break;
            case BymlNodeType.Float:
                int bytesWritten = Encoding.UTF8.GetBytes(byml.GetFloat().ToString("0.0############"), formattedFloatBuffer);
                emitter.WriteScalar(formattedFloatBuffer[..bytesWritten]);
                break;
            case BymlNodeType.UInt32:
                emitter.Tag("!u32");
                emitter.WriteUInt32(byml.GetUInt32());
                break;
            case BymlNodeType.Int64:
                emitter.Tag("!s64");
                emitter.WriteInt64(byml.GetInt64());
                break;
            case BymlNodeType.UInt64:
                emitter.Tag("!u64");
                emitter.WriteUInt64(byml.GetUInt64());
                break;
            case BymlNodeType.Double:
                emitter.Tag("!d");
                emitter.WriteDouble(byml.GetDouble());
                break;
            case BymlNodeType.Null:
                emitter.WriteNull();
                break;
            default:
                throw new InvalidOperationException($"""
                    Invalid or unsupported node type '{byml.Type}'
                    """);
        }
    }

    public static void WriteRawString(ref Utf8YamlEmitter emitter, int index, in ImmutableBymlStringTable stringTable)
    {
        emitter.WriteScalar(stringTable[index][..^1]);
    }

    public static void Write(ref Utf8YamlEmitter emitter, in Byml byml)
    {
        switch (byml.Value) {
            case IBymlNode node:
                node.EmitYaml(ref emitter);
                return;
        }

        byte[] formattedFloatRentedBuffer = ArrayPool<byte>.Shared.Rent(12);
        Span<byte> formattedFloatBuffer = formattedFloatRentedBuffer.AsSpan()[..12];

        switch (byml.Type) {
            case BymlNodeType.String:
                emitter.WriteString(byml.GetString());
                break;
            case BymlNodeType.Binary:
                byte[] data = byml.GetBinary();
                WriteBinary(ref emitter, data);
                break;
            case BymlNodeType.BinaryAligned:
                (byte[] dataAligned, int alignment) = byml.GetBinaryAligned();
                WriteBinaryAligned(ref emitter, dataAligned, alignment);
                break;
            case BymlNodeType.Bool:
                emitter.WriteBool(byml.GetBool());
                break;
            case BymlNodeType.Int:
                emitter.WriteInt32(byml.GetInt());
                break;
            case BymlNodeType.Float:
                int bytesWritten = Encoding.UTF8.GetBytes(byml.GetFloat().ToString("0.0############"), formattedFloatBuffer);
                emitter.WriteScalar(formattedFloatBuffer[..bytesWritten]);
                break;
            case BymlNodeType.UInt32:
                emitter.Tag("!u32");
                emitter.WriteUInt32(byml.GetUInt32());
                break;
            case BymlNodeType.Int64:
                emitter.Tag("!s64");
                emitter.WriteInt64(byml.GetInt64());
                break;
            case BymlNodeType.UInt64:
                emitter.Tag("!u64");
                emitter.WriteUInt64(byml.GetUInt64());
                break;
            case BymlNodeType.Double:
                emitter.Tag("!d");
                emitter.WriteDouble(byml.GetDouble());
                break;
            case BymlNodeType.Null:
                emitter.WriteNull();
                break;
            default:
                throw new InvalidOperationException($"""
                    Invalid or unsupported node type '{byml.Type}'
                    """);
        }
    }

    private static void WriteBinary(ref Utf8YamlEmitter emitter, Span<byte> data)
    {
        emitter.Tag("!!binary");
        emitter.WriteString(
            Convert.ToBase64String(data)
        );
    }

    private static void WriteBinaryAligned(ref Utf8YamlEmitter emitter, in Span<byte> data, int alignment)
    {
        emitter.Tag("!!file");
        emitter.BeginMapping(MappingStyle.Flow);
        emitter.WriteString("Alignment");
        emitter.WriteInt32(alignment);
        emitter.WriteString("Data");
        emitter.Tag("!!binary");
        emitter.WriteString(Convert.ToBase64String(data));
        emitter.EndMapping();
    }
}
