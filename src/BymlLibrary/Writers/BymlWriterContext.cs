using BymlLibrary.Extensions;
using BymlLibrary.Nodes.Containers;
using BymlLibrary.Structures;
using Revrs;
using System.Runtime.CompilerServices;

namespace BymlLibrary.Writers;

internal class BymlWriterContext
{
    private const ushort BYML_MAGIC_BE = 0x5942;

    private readonly Byml _root;
    private readonly ushort _version;

    private readonly Dictionary<Byml, int> _referenceNodes = [];
    private readonly Dictionary<int, int> _nodeOffsets = [];
    private readonly Stack<(long, Byml)> _staged = [];
    private int _trackAllStaged = 0;

    public RevrsWriter Writer { get; }
    public List<string> Keys { get; private set; } = [];
    public List<string> Strings { get; private set; } = [];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BymlWriterContext(Byml byml, in Stream stream, Endianness endianness, ushort version = 7)
    {
        Writer = new(stream, endianness);
        _version = version;
        Collect(_root = byml);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write()
    {
        Writer.Seek(BymlHeader.SIZE);

        int keyTableOffset = BymlStringTable.Write(this,
            Keys = [..Keys.Distinct().Order(StringComparer.Ordinal)]);
        int stringTableOffset = BymlStringTable.Write(this,
            Strings = [..Strings.Distinct().Order(StringComparer.Ordinal)]);
        int rootNodeOffset = (int)Writer.Position;

        if (_root._value is not (null or IBymlNode)) {
            throw new InvalidOperationException("""
                Root node must be a container type or null.
                """);
        }

        Write(_root);

        Writer.Seek(0);
        Writer.Write<BymlHeader, BymlHeader.Reverser>(new(
            magic: Writer.Endianness == Endianness.Little
                ? Byml.BYML_MAGIC : BYML_MAGIC_BE,
            _version,
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
        int staged = container.Write(this);

        for (int i = 0; i < staged; i++) {
            (long offset, Byml node) = _staged.Pop();
            if (!_referenceNodes.TryGetValue(node, out int hash)) {
                throw new InvalidOperationException($"""
                    Collection failed to collect '{node}'
                    """);
            }

            int currentPosition = (int)Writer.Position;
            if (_nodeOffsets.TryGetValue(hash, out int cachedOffset)) {
                Writer.Seek(offset);
                Writer.Write(cachedOffset);
                Writer.Seek(currentPosition);
            }
            else {
                _nodeOffsets.Add(hash, currentPosition);
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
                Writer.Write(
                    Strings.IndexOf(byml.GetString())
                );
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
    public void WriteContainerNode(in Byml byml)
    {
        _trackAllStaged++;

        if (byml.Type.IsValueType()) {
            WriteValue(byml);
        }
        else {
            _staged.Push((Writer.Position, byml));
            Writer.Move(4);
        }
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
            AddString(str);
            return str.GetHashCode();
        }
        else {
            return byml._value?.GetHashCode() ?? 0;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddKey(string value) => Keys.Add(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddString(string value) => Strings.Add(value);
}
