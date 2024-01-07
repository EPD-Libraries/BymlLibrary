using BymlLibrary.Extensions;
using BymlLibrary.Nodes.Immutable.Containers;
using BymlLibrary.Structures;
using Revrs;
using Revrs.Extensions;
using System.Runtime.CompilerServices;
using System.Text;

namespace BymlLibrary;

public readonly ref struct ImmutableByml
{
    public readonly BymlNodeType Type;
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
    private readonly int _offset;

    public ImmutableByml(ref RevrsReader reader)
    {
        _data = reader.Data;

        reader.Endianness = Endianness.Little;
        Header = reader.Read<BymlHeader, BymlHeader.Reverser>();

        if (Header.Magic != Byml.BYML_MAGIC_LE) {
            reader.Endianness = Endianness.Big;
            Header = reader.Read<BymlHeader, BymlHeader.Reverser>();
        }

        if (Header.Magic is not (Byml.BYML_MAGIC or Byml.BYML_MAGIC_LE)) {
            throw new InvalidDataException(
                $"Invalid BYML magic: '{Encoding.UTF8.GetString(BitConverter.GetBytes(Header.Magic))}'");
        }

        if (Header.Version < 2 || Header.Version > 7) {
            throw new InvalidDataException(
                $"Unsupported BYML version: '{Header.Version}'");
        }

        if (Header.KeyTableOffset > 0) {
            ref BymlContainerNodeHeader keyTableHeader
                = ref CheckContainerHeader(BymlNodeType.StringTable, Header.KeyTableOffset);
            KeyTable = new(_data, Header.KeyTableOffset, keyTableHeader.Count);
        }

        if (Header.StringTableOffset > 0) {
            ref BymlContainerNodeHeader stringTableHeader
                = ref CheckContainerHeader(BymlNodeType.StringTable, Header.StringTableOffset);
            StringTable = new(_data, Header.StringTableOffset, stringTableHeader.Count);
        }

        ref BymlContainerNodeHeader rootNodeHeader
            = ref _data[(_offset = Header.RootNodeOffset)..].Read<BymlContainerNodeHeader>();
        Type = rootNodeHeader.Type;
    }

    internal ImmutableByml(Span<byte> data, int value, BymlNodeType type)
    {
        _data = data;
        _offset = value;
        Type = type;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ImmutableBymlHashMap GetHashMap()
    {
        ref BymlContainerNodeHeader header
            = ref CheckContainerHeader(BymlNodeType.HashMap);
        return new ImmutableBymlHashMap(_data, _offset, header.Count, header.Type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ImmutableBymlArray GetArray()
    {
        ref BymlContainerNodeHeader header
            = ref CheckContainerHeader(BymlNodeType.Array);
        return new ImmutableBymlArray(_data, _offset, header.Count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ImmutableBymlMap GetMap()
    {
        ref BymlContainerNodeHeader header
            = ref CheckContainerHeader(BymlNodeType.Map);
        return new ImmutableBymlMap(_data, _offset, header.Count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ImmutableBymlStringTable GetStringTable()
    {
        ref BymlContainerNodeHeader header
            = ref CheckContainerHeader(BymlNodeType.StringTable);
        return new(_data, _offset, header.Count);
    }

    //
    // Special Value Types

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Span<byte> GetBinary()
    {
        int size = _data[_offset..].Read<int>();
        int offset = _offset + sizeof(int);
        return _data[offset..(offset + size)];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Span<byte> GetBinaryAligned(out int alignment)
    {
        int size = _data[_offset..].Read<int>();
        alignment = _data[(_offset + sizeof(int))..].Read<int>();

        int offset = _offset + sizeof(int) * 2;
        return _data[offset..(offset + size)];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int GetStringIndex()
    {
        Type.Assert(BymlNodeType.String);
        return _offset;
    }

    //
    // Value Types

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool GetBool()
    {
        Type.Assert(BymlNodeType.Bool);
        return _offset != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int GetInt()
    {
        Type.Assert(BymlNodeType.Int);
        return _offset;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float GetFloat()
    {
        Type.Assert(BymlNodeType.Float);
        return _offset;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly uint GetUInt32()
    {
        Type.Assert(BymlNodeType.UInt32);
        return (uint)_offset;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref long GetInt64()
    {
        Type.Assert(BymlNodeType.Int64);
        return ref _data[_offset..].Read<long>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref ulong GetUInt64()
    {
        Type.Assert(BymlNodeType.UInt64);
        return ref _data[_offset..].Read<ulong>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref double GetDouble()
    {
        Type.Assert(BymlNodeType.Double);
        return ref _data[_offset..].Read<double>();
    }

    public bool IsNull {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Type == BymlNodeType.Null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ReverseNode(ref RevrsReader reader, int value, BymlNodeType type)
    {
        if (type.IsContainerType()) {
            ReverseContainer(ref reader, value);
        }
        else if (type.IsSpecialValueType()) {
            ReverseSpecialValue(ref reader, value, type);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ReverseContainer(ref RevrsReader reader, int offset)
    {
        BymlContainerNodeHeader header = reader
            .Read<BymlContainerNodeHeader>(offset);

        if (header.Type == BymlNodeType.HashMap) {
            ImmutableBymlHashMap.Reverse(ref reader, offset, header.Count);
        }
        else if (header.Type == BymlNodeType.RemappedHashMap) {
            throw new NotImplementedException();
        }
        else if (header.Type == BymlNodeType.Array) {
            ImmutableBymlArray.Reverse(ref reader, offset, header.Count);
        }
        else if (header.Type == BymlNodeType.Map) {
            ImmutableBymlMap.Reverse(ref reader, offset, header.Count);
        }
        else if (header.Type == BymlNodeType.StringTable) {
            ImmutableBymlStringTable.Reverse(ref reader, offset, header.Count);
        }
        else if (header.Type == BymlNodeType.RemappedMap) {
            throw new NotImplementedException();
        }
        else if (header.Type == BymlNodeType.RelocatedStringTable) {
            throw new NotImplementedException();
        }
        else if (header.Type == BymlNodeType.MonoTypedArray) {
            throw new NotImplementedException();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ReverseSpecialValue(ref RevrsReader reader, int offset, BymlNodeType type)
    {
        if (type is BymlNodeType.String or BymlNodeType.Binary) {
            reader.Reverse<int>(offset);
        }
        else if (type is BymlNodeType.BinaryAligned) {
            reader.Reverse<int>(offset);
            reader.Reverse<int>();
        }
        else if (type is BymlNodeType.Int64 or BymlNodeType.UInt64 or BymlNodeType.Double) {
            reader.Reverse<ulong>(offset);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref BymlContainerNodeHeader CheckContainerHeader(BymlNodeType expected)
    {
        ref BymlContainerNodeHeader header = ref _data[_offset..]
            .Read<BymlContainerNodeHeader>();
        header.Type.Assert(expected);
        return ref header;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref BymlContainerNodeHeader CheckContainerHeader(BymlNodeType expected, int offset)
    {
        ref BymlContainerNodeHeader header = ref _data[offset..]
            .Read<BymlContainerNodeHeader>();
        header.Type.Assert(expected);
        return ref header;
    }
}
