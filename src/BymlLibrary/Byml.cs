using BymlLibrary.Extensions;
using BymlLibrary.Nodes.Containers;
using BymlLibrary.Nodes.Containers.HashMap;
using BymlLibrary.Writers;
using BymlLibrary.Yaml;
using Revrs;
using Revrs.Buffers;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using VYaml.Emitter;

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
    public static class YamlConfig
    {
        /// <summary>
        /// The max amount of children in a container node to trigger a flow style scalar (inline)
        /// </summary>
        public static int InlineContainerMaxCount { get; set; } = 8;
    }

    /// <summary>
    /// <c>YB</c>
    /// </summary>
    internal const ushort BYML_MAGIC = 0x4259;

    public readonly object? Value;

    public BymlNodeType Type { get; set; }

    public static Byml FromBinary(Span<byte> data)
    {
        RevrsReader reader = new(data);
        ImmutableByml byml = new(ref reader);
        return FromImmutable(byml, byml);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Byml FromText(string text)
    {
        int size = Encoding.UTF8.GetByteCount(text);
        using ArraySegmentOwner<byte> utf8 = ArraySegmentOwner<byte>.Allocate(size);
        Encoding.UTF8.GetBytes(text, utf8.Segment);
        return BymlYamlReader.Parse(utf8.Segment);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Byml FromText(ArraySegment<byte> utf8Text)
    {
        return BymlYamlReader.Parse(utf8Text);
    }

    public static Byml FromImmutable(in ImmutableByml root)
    {
        return FromImmutable(root, root);
    }

    public static Byml FromImmutable(in ImmutableByml byml, in ImmutableByml root)
    {
        Byml result = byml.Type switch {
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

        return result;
    }

    public byte[] ToBinary(Endianness endianness, ushort version = 2)
    {
        MemoryStream ms = new();
        WriteBinary(ms, endianness, version);
        return ms.ToArray();
    }

    /// <summary>
    /// <b>Warning:</b> use <c>ToBinary</c> or <c>WriteBinary(string, ...)</c> when writing to a file,<br/>
    /// the writting process is much faster when written into memory and then copied to disk.
    /// </summary>
    /// <param name="stream">The stream to write into (must be seekable)</param>
    /// <param name="endianness">The endianness to use when writing the file</param>
    /// <param name="version">The BYML version to use when writing the file</param>
    public void WriteBinary(in Stream stream, Endianness endianness, ushort version = 2)
    {
        BymlWriter writer = new(this, stream, endianness, version);
        writer.Write();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBinary(string filename, Endianness endianness, ushort version = 2)
    {
        File.WriteAllBytes(filename, ToBinary(endianness, version));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToYaml()
    {
        ArrayBufferWriter<byte> writer = new();
        WriteYaml(writer);
        return Encoding.UTF8.GetString(writer.WrittenSpan);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteYaml(IBufferWriter<byte> writer)
    {
        Utf8YamlEmitter emitter = new(writer);
        BymlYamlWriter.Write(ref emitter, this);
    }

    public static implicit operator Byml(Dictionary<uint, Byml> hashMap32) => new(hashMap32);
    public Byml(IDictionary<uint, Byml> hashMap32)
    {
        Type = BymlNodeType.HashMap32;
        Value = new BymlHashMap32(hashMap32);
    }

    public static implicit operator Byml(BymlHashMap32 hashMap32) => new(hashMap32);
    public Byml(BymlHashMap32 hashMap32)
    {
        Type = BymlNodeType.HashMap32;
        Value = hashMap32;
    }

    public static implicit operator Byml(Dictionary<ulong, Byml> hashMap64) => new(hashMap64);
    public Byml(IDictionary<ulong, Byml> hashMap64)
    {
        Type = BymlNodeType.HashMap64;
        Value = new BymlHashMap64(hashMap64);
    }

    public static implicit operator Byml(BymlHashMap64 hashMap64) => new(hashMap64);
    public Byml(BymlHashMap64 hashMap64)
    {
        Type = BymlNodeType.HashMap64;
        Value = hashMap64;
    }

    public static implicit operator Byml(Byml[] array) => new(array);
    public Byml(IEnumerable<Byml> array)
    {
        Type = BymlNodeType.HashMap32;
        Value = new BymlArray(array);
    }

    public static implicit operator Byml(BymlArray array) => new(array);
    public Byml(BymlArray array)
    {
        Type = BymlNodeType.Array;
        Value = array;
    }

    public static implicit operator Byml(Dictionary<string, Byml> map) => new(map);
    public Byml(IDictionary<string, Byml> map)
    {
        Type = BymlNodeType.HashMap32;
        Value = new BymlMap(map);
    }

    public static implicit operator Byml(BymlMap map) => new(map);
    public Byml(BymlMap map)
    {
        Type = BymlNodeType.Map;
        Value = map;
    }

    public static implicit operator Byml(string value) => new(value);
    public Byml(string value)
    {
        Type = BymlNodeType.String;
        Value = value;
    }

    public static implicit operator Byml(byte[] data) => new(data);
    public Byml(byte[] data)
    {
        Type = BymlNodeType.Binary;
        Value = data;
    }

    public static implicit operator Byml((byte[] data, int alignment) value) => new(value.data, value.alignment);
    public Byml(byte[] data, int alignment)
    {
        Type = BymlNodeType.BinaryAligned;
        Value = (data, alignment);
    }

    public static implicit operator Byml(bool value) => new(value);
    public Byml(bool value)
    {
        Type = BymlNodeType.Bool;
        Value = value;
    }

    public static implicit operator Byml(int value) => new(value);
    public Byml(int value)
    {
        Type = BymlNodeType.Int;
        Value = value;
    }

    public static implicit operator Byml(float value) => new(value);
    public Byml(float value)
    {
        Type = BymlNodeType.Float;
        Value = value;
    }

    public static implicit operator Byml(uint value) => new(value);
    public Byml(uint value)
    {
        Type = BymlNodeType.UInt32;
        Value = value;
    }

    public static implicit operator Byml(long value) => new(value);
    public Byml(long value)
    {
        Type = BymlNodeType.Int64;
        Value = value;
    }

    public static implicit operator Byml(ulong value) => new(value);
    public Byml(ulong value)
    {
        Type = BymlNodeType.UInt64;
        Value = value;
    }

    public static implicit operator Byml(double value) => new(value);
    public Byml(double value)
    {
        Type = BymlNodeType.Double;
        Value = value;
    }

    public Byml()
    {
        Type = BymlNodeType.Null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BymlHashMap32 GetHashMap32()
        => Get<BymlHashMap32>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BymlHashMap64 GetHashMap64()
        => Get<BymlHashMap64>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BymlArray GetArray()
        => Get<BymlArray>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BymlMap GetMap()
        => Get<BymlMap>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetString()
        => Get<string>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] GetBinary()
        => Get<byte[]>();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (byte[] Data, int Alignment) GetBinaryAligned()
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
        if (Value is null) {
            throw new InvalidOperationException($"""
                Cannot parse null node
                """);
        }

        if (Value is T value) {
            return value;
        }

        throw new InvalidDataException($"""
            Unexpected type: '{typeof(T)}'

            Expected '{Value.GetType()} ({Type})' but found '{typeof(T)}'
            """);
    }

    public class ValueEqualityComparer : IEqualityComparer<Byml>
    {
        private static readonly BymlHashMap32.ValueEqualityComparer _hashMap32Comparer = new();
        private static readonly BymlHashMap64.ValueEqualityComparer _hashMap64Comparer = new();
        private static readonly BymlArray.ValueEqualityComparer _arrayComparer = new();
        private static readonly BymlMap.ValueEqualityComparer _mapComparer = new();
        private static readonly ValueEqualityComparer _default = new();

        public static ValueEqualityComparer Default => _default;

        public bool Equals(Byml? x, Byml? y)
        {
            if (x?.Type != y?.Type) {
                return false;
            }

            if (x?.Value == y?.Value) {
                return true;
            }

            if (x?.Value is null || y?.Value is null) {
                return false;
            }

            return x.Type switch {
                BymlNodeType.HashMap32 => _hashMap32Comparer.Equals(x.GetHashMap32(), y.GetHashMap32()),
                BymlNodeType.HashMap64 => _hashMap64Comparer.Equals(x.GetHashMap64(), y.GetHashMap64()),
                BymlNodeType.Map => _mapComparer.Equals(x.GetMap(), y.GetMap()),
                BymlNodeType.Array => _arrayComparer.Equals(x.GetArray(), y.GetArray()),
                BymlNodeType.String => x.Value.GetHashCode() == y.Value.GetHashCode(),
                BymlNodeType.Binary => x.GetBinary().SequenceEqual(y.GetBinary()),
                BymlNodeType.BinaryAligned => CompareBinaryAligned(x.GetBinaryAligned(), y.GetBinaryAligned()),
                BymlNodeType.Bool => x.GetBool() == y.GetBool(),
                BymlNodeType.Int => x.GetInt() == y.GetInt(),
                BymlNodeType.UInt32 => x.GetUInt32() == y.GetUInt32(),
                BymlNodeType.Float => x.GetFloat() == y.GetFloat(),
                BymlNodeType.Int64 => x.GetInt64() == y.GetInt64(),
                BymlNodeType.UInt64 => x.GetUInt64() == y.GetUInt64(),
                BymlNodeType.Double => x.GetDouble() == y.GetDouble(),
                _ => throw new NotImplementedException($"""
                    A comparer for the node type '{x.Type}' is not implemented
                    """),
            };
        }

        public int GetHashCode([DisallowNull] Byml byml)
        {
            if (byml.Value is IBymlNode container) {
                return container.GetValueHash();
            }
            else {
                return byml.Type switch {
                    BymlNodeType.Binary => GetBinaryNodeHashCode((byml.GetBinary(), null), byml.Type),
                    BymlNodeType.BinaryAligned => GetBinaryNodeHashCode(byml.GetBinaryAligned(), byml.Type),
                    _ => GetValueNodeHashCode(byml)
                };
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBinaryNodeHashCode((byte[] data, int? alignment) value, BymlNodeType bymlNodeType)
        {
            HashCode hashCode = new();
            if (value.alignment.HasValue) {
                hashCode.Add(value.alignment.Value);
            }

            hashCode.Add(bymlNodeType.GetHashCode());
            hashCode.AddBytes(value.data);
            return hashCode.ToHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetValueNodeHashCode(Byml byml)
        {
            HashCode hashCode = new();
            hashCode.Add(byml.Type.GetHashCode());
            hashCode.Add(byml.Value?.GetHashCode());
            return hashCode.ToHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool CompareBinaryAligned((byte[] data, int alignment) x, (byte[] data, int alignment) y)
        {
            return x.alignment == y.alignment && x.data.SequenceEqual(y.data);
        }
    }
}
