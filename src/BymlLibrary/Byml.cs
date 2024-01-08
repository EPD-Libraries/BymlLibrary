using BymlLibrary.Extensions;
using Revrs;
using System.Runtime.CompilerServices;

namespace BymlLibrary;

public enum BymlNodeType : byte
{
    // A better solution could be
    // used for handling these map tyes
    HashMap32 = 0x20,
    HashMap64 = 0x21,
    RelocatedHashMap32 = 0x30, // Unknown
    RelocatedHashMap64 = 0x31, // Unknown
    String = 0xA0,
    Binary = 0xA1,
    BinaryAligned = 0xA2,
    Array = 0xC0,
    Map = 0xC1,
    StringTable = 0xC2,
    RemappedMap = 0xC4, // Unknown
    RelocatedStringTable = 0xC5, // Unknown
    MonoTypedArray = 0xC8, // Unknown

    // Value Types
    Bool = 0xD0,
    Int = 0xD1,
    Float = 0xD2,
    UInt32 = 0xD3,
    Int64 = 0xD4,
    UInt64 = 0xD5,
    Double = 0xD6,
    Null = 0xFF,
}

public sealed class Byml
{
    /// <summary>
    /// <c>BY</c>
    /// </summary>
    internal const ushort BYML_MAGIC = 0x5942;

    /// <summary>
    /// <c>YB</c>
    /// </summary>
    internal const ushort BYML_MAGIC_LE = 0x4259;

    private readonly object? _value;

    public BymlNodeType Type { get; set; }

    public static Byml FromBinary(Span<byte> data)
    {
        RevrsReader reader = new(data);
        ImmutableByml byml = new(ref reader);
        return FromImmutable(byml, byml);
    }

    public static Byml FromImmutable(in ImmutableByml byml, in ImmutableByml root)
    {
        return byml.Type switch {
            BymlNodeType.HashMap32
                => new(byml.GetHashMap32().ToMutable(root)),
            BymlNodeType.HashMap64
                => new(byml.GetHashMap64().ToMutable(root)),
            BymlNodeType.String
                => new(root.StringTable[byml.GetStringIndex()].ToManaged()),
            BymlNodeType.Binary
                => new(byml.GetBinary().ToArray()),
            BymlNodeType.BinaryAligned
                => new(byml.GetBinaryAligned(out int alignment).ToArray(), alignment),
            BymlNodeType.Array
                => new(byml.GetArray().ToMutable(root)),
            BymlNodeType.Map
                => new(byml.GetMap().ToMutable(root)),
            BymlNodeType.Bool
                => new(byml.GetBool()),
            BymlNodeType.Int
                => new(byml.GetInt()),
            BymlNodeType.Float
                => new(byml.GetFloat()),
            BymlNodeType.UInt32
                => new(byml.GetUInt32()),
            BymlNodeType.Int64
                => new(byml.GetInt64()),
            BymlNodeType.UInt64
                => new(byml.GetUInt64()),
            BymlNodeType.Double
                => new(byml.GetDouble()),
            BymlNodeType.Null
                => new(),

            _ => throw new InvalidDataException($"""
                Invalid or unsupported node type '{byml.Type}'
                """)
        };
    }

    public Byml(SortedDictionary<uint, Byml>? hashMap32)
    {
        Type = BymlNodeType.HashMap32;
        _value = hashMap32;
    }

    public Byml(SortedDictionary<ulong, Byml>? hashMap64)
    {
        Type = BymlNodeType.HashMap64;
        _value = hashMap64;
    }

    public Byml(List<Byml>? array)
    {
        Type = BymlNodeType.Array;
        _value = array;
    }

    public Byml(SortedDictionary<string, Byml>? map)
    {
        Type = BymlNodeType.Map;
        _value = map;
    }

    public Byml(string value)
    {
        Type = BymlNodeType.String;
        _value = value;
    }

    public Byml(byte[] data)
    {
        Type = BymlNodeType.Binary;
        _value = data;
    }

    public Byml(byte[] data, int alignment)
    {
        Type = BymlNodeType.BinaryAligned;
        _value = (data, alignment);
    }

    public Byml(bool value)
    {
        Type = BymlNodeType.Bool;
        _value = value;
    }

    public Byml(int value)
    {
        Type = BymlNodeType.Int;
        _value = value;
    }

    public Byml(float value)
    {
        Type = BymlNodeType.Float;
        _value = value;
    }

    public Byml(uint value)
    {
        Type = BymlNodeType.UInt32;
        _value = value;
    }

    public Byml(long value)
    {
        Type = BymlNodeType.Int64;
        _value = value;
    }

    public Byml(ulong value)
    {
        Type = BymlNodeType.UInt64;
        _value = value;
    }

    public Byml(double value)
    {
        Type = BymlNodeType.Double;
        _value = value;
    }

    public Byml()
    {
        Type = BymlNodeType.Null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SortedDictionary<uint, Byml> GetHashMap32()
        => Get<SortedDictionary<uint, Byml>>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SortedDictionary<ulong, Byml> GetHashMap64()
        => Get<SortedDictionary<ulong, Byml>>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public List<Byml> GetArray()
        => Get<List<Byml>>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SortedDictionary<string, Byml> GetMap()
        => Get<SortedDictionary<string, Byml>>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetString()
        => Get<string>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] GetBinary()
        => Get<byte[]>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (byte[], int) GetBinaryAligned()
        => Get<(byte[], int)>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool GetBool()
        => Get<bool>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetInt()
        => Get<int>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float GetFloat()
        => Get<float>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint GetUInt32()
        => Get<uint>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long GetInt64()
        => Get<long>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong GetUInt64()
        => Get<ulong>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double GetDouble()
        => Get<double>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Get<T>()
    {
        if (_value is null) {
            throw new InvalidOperationException($"""
                Cannot parse null node
                """);
        }

        if (_value is T value) {
            return value;
        }

        throw new InvalidDataException($"""
            Unexpected type: '{typeof(T)}'

            Expected '{_value.GetType()} ({Type})' but found '{typeof(T)}'
            """);
    }
}
