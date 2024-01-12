using BymlLibrary.Extensions;
using BymlLibrary.Nodes.Containers;
using BymlLibrary.Structures;
using Revrs;
using System.Runtime.CompilerServices;

namespace BymlLibrary.Writers;

internal class BymlWriter
{
    private readonly Byml _root;
    private readonly ushort _version;

    private readonly Dictionary<Byml, int> _referenceNodes = [];
    private readonly Dictionary<int, int> __referenceNodeOffsets = [];

    private Dictionary<string, int> _keys = [];
    private Dictionary<string, int> _strings = [];

    public RevrsWriter Writer { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BymlWriter(Byml byml, in Stream stream, Endianness endianness, ushort version = 7)
    {
        Writer = new(stream, endianness);
        _version = version;
        Collect(_root = byml);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write()
    {
        Writer.Seek(BymlHeader.SIZE);

        int keyTableOffset = WriteStringTable(ref _keys);
        int stringTableOffset = WriteStringTable(ref _strings);
        int rootNodeOffset = (int)Writer.Position;

        if (_root._value is not (null or IBymlNode)) {
            throw new InvalidOperationException("""
                Root node must be a container type or null.
                """);
        }

        Write(_root);

        Writer.Seek(0);
        Writer.Write<BymlHeader, BymlHeader.Reverser>(new(
            magic: Byml.BYML_MAGIC,
            version: _version,
            keyTableOffset,
            stringTableOffset,
            rootNodeOffset
        ));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(in Byml byml)
    {
        if (byml._value is IBymlNode container) {
            WriteContainer(container);
        }
        else if (byml.Type.IsSpecialValueType()) {
            WriteSpecial(byml);
        }
        else {
            WriteValue(byml);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void WriteContainer(IBymlNode container)
    {
        List<(long, Byml)> staged = [];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void WriteNode(Byml byml)
        {
            if (byml.Type.IsValueType()) {
                WriteValue(byml);
            }
            else {
                staged.Add((Writer.Position, byml));
                Writer.Write(0u);
            }
        }

        container.Write(this, WriteNode);

        foreach ((long offset, Byml node) in staged) {
            if (!_referenceNodes.TryGetValue(node, out int hash)) {
                throw new InvalidOperationException($"""
                    Collection failed to collect '{node}'
                    """);
            }

            int currentPosition = (int)Writer.Position;
            if (__referenceNodeOffsets.TryGetValue(hash, out int cachedOffset)) {
                Writer.Seek(offset);
                Writer.Write(cachedOffset);
                Writer.Seek(currentPosition);
            }
            else {
                __referenceNodeOffsets.Add(hash, currentPosition);
                Writer.Seek(offset);
                Writer.Write(currentPosition);
                Writer.Seek(currentPosition);
                Write(node);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteValue(in Byml byml)
    {
        switch (byml.Type) {
            case BymlNodeType.String:
                Writer.Write(_strings[byml.GetString()]);
                break;
            case BymlNodeType.Bool:
                Writer.Write(byml.GetBool());
                Writer.Move(3);
                break;
            case BymlNodeType.Int:
                Writer.Write(byml.GetInt());
                break;
            case BymlNodeType.UInt32:
                Writer.Write(byml.GetUInt32());
                break;
            case BymlNodeType.Float:
                Writer.Write(byml.GetFloat());
                break;
            case BymlNodeType.Null:
                Writer.Write(0);
                break;
            default:
                throw new NotSupportedException($"""
                    The value type node '{byml.Type}' is not supported.
                    """);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteSpecial(in Byml byml)
    {
        switch (byml.Type) {
            case BymlNodeType.Binary:
                byte[] data = byml.GetBinary();
                Writer.Write(data.Length);
                Writer.Write(data);
                break;
            case BymlNodeType.BinaryAligned:
                (byte[] alignedData, int alignment) = byml.GetBinaryAligned();
                Writer.Write(alignedData.Length);
                Writer.Write(alignment);
                Writer.Write(alignedData);
                break;
            case BymlNodeType.Int64 or BymlNodeType.UInt64 or BymlNodeType.Double:
                Writer.Write((ulong)(byml._value ?? 0));
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteContainerHeader(BymlNodeType type, int count)
    {
        BymlContainer container = new(type, count);
        Writer.Write<BymlContainer, BymlContainer.Reverser>(container);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int WriteStringTable(ref Dictionary<string, int> strings)
    {
        strings = strings
            .OrderBy(x => x.Key, StringComparer.Ordinal)
            .Select((x, i) => (x.Key, Index: i))
            .ToDictionary(x => x.Key, x => x.Index);

        int tableOffset = (int)Writer.Position;

        WriteContainerHeader(BymlNodeType.StringTable, strings.Count);

        int previousStringOffset = (strings.Count + 1) * sizeof(uint) + BymlContainer.SIZE;
        Writer.Write(previousStringOffset);
        foreach (var str in strings.Keys) {
            Writer.Write(previousStringOffset += str.Length + 1);
        }

        foreach (var str in strings.Keys) {
            Writer.WriteStringUtf8(str);
            Writer.Write<byte>(0);
        }

        Writer.Align(4);
        return tableOffset;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Collect(in Byml byml)
    {
        if (byml._value is IBymlNode container) {
            int hash = container.Collect(this);
            _referenceNodes[byml] = hash;
            return hash;
        }
        else if (byml._value is string str) {
            _strings[str] = 0;
            return str.GetHashCode();
        }
        else {
            return byml._value?.GetHashCode() ?? 0;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddKey(string value) => _keys[value] = 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetKeyIndex(string key) => _keys[key];
}
