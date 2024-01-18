using BymlLibrary.Extensions;
using BymlLibrary.Nodes.Immutable.Containers;
using BymlLibrary.Nodes.Immutable.Containers.HashMap;
using BymlLibrary.Structures;
using BymlLibrary.Yaml;
using Revrs;
using Revrs.Extensions;
using System.Runtime.CompilerServices;
using System.Text;

namespace BymlLibrary;

public readonly ref struct ImmutableByml
{
    public readonly BymlNodeType Type;
    public readonly Endianness Endianness;
    public readonly BymlHeader Header;
    public readonly ImmutableBymlStringTable KeyTable;
    public readonly ImmutableBymlStringTable StringTable;

    /// <summary>
    /// A span of the byml data
    /// </summary>
    private readonly Span<byte> _data;

    /// <summary>
    /// The node offset, or the actual value in value nodes
    /// </summary>
    private readonly BymlValue _value;

    public ImmutableByml(ref RevrsReader reader)
    {
        _data = reader.Data;

        reader.Endianness = Endianness.Little;
        Header = reader.Read<BymlHeader, BymlHeader.Reverser>();

        if (Header.Magic != Byml.BYML_MAGIC) {
            reader.Endianness = Endianness.Big;
            Header = reader.Read<BymlHeader, BymlHeader.Reverser>(0);
        }

        if (Header.Magic != Byml.BYML_MAGIC) {
            throw new InvalidDataException(
                $"Invalid BYML magic: '{Encoding.UTF8.GetString(BitConverter.GetBytes(Header.Magic))}'");
        }

        if (Header.Version < 2 || Header.Version > 7) {
            throw new InvalidDataException(
                $"Unsupported BYML version: '{Header.Version}'");
        }

        if (Header.KeyTableOffset > 0) {
            reader.Seek(Header.KeyTableOffset);
            ref BymlContainer keyTableHeader
                = ref CheckContainerHeader(ref reader, BymlNodeType.StringTable);
            KeyTable = new(_data, Header.KeyTableOffset, keyTableHeader.Count);

            if (reader.Endianness.IsNotSystemEndianness()) {
                ImmutableBymlStringTable.Reverse(ref reader, Header.KeyTableOffset, keyTableHeader.Count);
            }
        }

        if (Header.StringTableOffset > 0) {
            reader.Seek(Header.StringTableOffset);
            ref BymlContainer stringTableHeader
                = ref CheckContainerHeader(ref reader, BymlNodeType.StringTable);
            StringTable = new(_data, Header.StringTableOffset, stringTableHeader.Count);

            if (reader.Endianness.IsNotSystemEndianness()) {
                ImmutableBymlStringTable.Reverse(ref reader, Header.KeyTableOffset, stringTableHeader.Count);
            }
        }

        if (reader.Endianness.IsNotSystemEndianness()) {
            // So much for 0 allocation :sadge:
            HashSet<int> reversedOffsets = [
                Header.KeyTableOffset,
                Header.StringTableOffset,
                Header.RootNodeOffset
            ];

            ReverseContainer(ref reader, Header.RootNodeOffset, reversedOffsets);
        }

        ref BymlContainer rootNodeHeader
            = ref _data[(_value = new(Header.RootNodeOffset)).Offset..].Read<BymlContainer>();
        Type = rootNodeHeader.Type;
        Endianness = reader.Endianness;
    }

    internal ImmutableByml(Span<byte> data, int value, BymlNodeType type)
    {
        _data = data;
        _value = new(value);
        Type = type;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToYaml()
    {
        YamlEmitter emitter = new();
        emitter.Emit(this);
        return emitter.Builder.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ImmutableBymlHashMap32 GetHashMap32()
    {
        ref BymlContainer header
            = ref CheckContainerHeader(BymlNodeType.HashMap32);
        return new ImmutableBymlHashMap32(_data, _value.Offset, header.Count, header.Type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ImmutableBymlHashMap64 GetHashMap64()
    {
        ref BymlContainer header
            = ref CheckContainerHeader(BymlNodeType.HashMap64);
        return new ImmutableBymlHashMap64(_data, _value.Offset, header.Count, header.Type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ImmutableBymlArray GetArray()
    {
        ref BymlContainer header
            = ref CheckContainerHeader(BymlNodeType.Array);
        return new ImmutableBymlArray(_data, _value.Offset, header.Count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ImmutableBymlMap GetMap()
    {
        ref BymlContainer header
            = ref CheckContainerHeader(BymlNodeType.Map);
        return new ImmutableBymlMap(_data, _value.Offset, header.Count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ImmutableBymlStringTable GetStringTable()
    {
        ref BymlContainer header
            = ref CheckContainerHeader(BymlNodeType.StringTable);
        return new(_data, _value.Offset, header.Count);
    }

    //
    // Special Value Types

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Span<byte> GetBinary()
    {
        int size = _data[_value.Offset..].Read<int>();
        int offset = _value.Offset + sizeof(int);
        return _data[offset..(offset + size)];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Span<byte> GetBinaryAligned(out int alignment)
    {
        int size = _data[_value.Offset..].Read<int>();
        alignment = _data[(_value.Offset + sizeof(int))..].Read<int>();

        int offset = _value.Offset + (sizeof(int) * 2);
        return _data[offset..(offset + size)];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int GetStringIndex()
    {
        Type.Assert(BymlNodeType.String);
        return _value.Int;
    }

    //
    // Value Types

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool GetBool()
    {
        Type.Assert(BymlNodeType.Bool);
        return _value.Bool;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int GetInt()
    {
        Type.Assert(BymlNodeType.Int);
        return _value.Int;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float GetFloat()
    {
        Type.Assert(BymlNodeType.Float);
        return _value.Float;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly uint GetUInt32()
    {
        Type.Assert(BymlNodeType.UInt32);
        return (uint)_value.UInt32;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref long GetInt64()
    {
        Type.Assert(BymlNodeType.Int64);
        return ref _data[_value.Offset..].Read<long>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref ulong GetUInt64()
    {
        Type.Assert(BymlNodeType.UInt64);
        return ref _data[_value.Offset..].Read<ulong>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref double GetDouble()
    {
        Type.Assert(BymlNodeType.Double);
        return ref _data[_value.Offset..].Read<double>();
    }

    public bool IsNull {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Type == BymlNodeType.Null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ReverseNode(ref RevrsReader reader, int value, BymlNodeType type, in HashSet<int> reversedOffsets)
    {
        if (type.IsValueType()) {
            return;
        }

        if (reversedOffsets.Contains(value)) {
            return;
        }

        reversedOffsets.Add(value);

        if (type.IsContainerType()) {
            ReverseContainer(ref reader, value, reversedOffsets);
        }
        else if (type.IsSpecialValueType()) {
            ReverseSpecialValue(ref reader, value, type);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ReverseContainer(ref RevrsReader reader, int offset, in HashSet<int> reversedOffsets)
    {
        BymlContainer header = reader
            .Read<BymlContainer, BymlContainer.Reverser>(offset);

        switch (header.Type) {
            case BymlNodeType.HashMap32:
                ImmutableBymlHashMap32.Reverse(ref reader, offset, header.Count, reversedOffsets);
                break;
            case BymlNodeType.HashMap64:
                ImmutableBymlHashMap64.Reverse(ref reader, offset, header.Count, reversedOffsets);
                break;
            case BymlNodeType.Array:
                ImmutableBymlArray.Reverse(ref reader, offset, header.Count, reversedOffsets);
                break;
            case BymlNodeType.Map:
                ImmutableBymlMap.Reverse(ref reader, offset, header.Count, reversedOffsets);
                break;
            default:
                throw new NotImplementedException($"""
                    The container type '{header.Type}' has no implemented reverser
                    """);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ReverseSpecialValue(ref RevrsReader reader, int offset, BymlNodeType type)
    {
        switch (type) {
            case BymlNodeType.String or BymlNodeType.Binary:
                reader.Reverse<int>(offset);
                break;
            case BymlNodeType.BinaryAligned:
                reader.Reverse<int>(offset);
                reader.Reverse<int>();
                break;
            case BymlNodeType.Int64 or BymlNodeType.UInt64 or BymlNodeType.Double:
                reader.Reverse<ulong>(offset);
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref BymlContainer CheckContainerHeader(BymlNodeType expected)
    {
        ref BymlContainer header = ref _data[_value.Offset..]
            .Read<BymlContainer>();
        header.Type.Assert(expected);
        return ref header;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref BymlContainer CheckContainerHeader(ref RevrsReader reader, BymlNodeType expected)
    {
        ref BymlContainer header
            = ref reader.Read<BymlContainer, BymlContainer.Reverser>();
        header.Type.Assert(expected);
        return ref header;
    }
}
