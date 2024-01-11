using BymlLibrary.Nodes.Containers;
using BymlLibrary.Structures;
using Revrs;
using System.Runtime.CompilerServices;

namespace BymlLibrary.Writers;

internal class BymlWriter
{
    private const ushort BYML_MAGIC_LE = 0x5942;

    private readonly Byml _root;
    private readonly ushort _version;

    private readonly RevrsWriter _writer;

    private readonly List<string> _keys = [];
    private readonly List<string> _strings = [];
    private readonly Dictionary<Byml, int> _referenceNodes = [];

    public BymlWriter(Byml byml, in Stream stream, Endianness endianness, ushort version = 7)
    {
        _writer = new(stream, endianness);
        _version = version;
        Collect(_root = byml);
    }

    public void Write()
    {
        _writer.Seek(BymlHeader.SIZE);

        int keyTableOffset = BymlStringTable.Write(_writer, _keys);
        int stringTableOffset = BymlStringTable.Write(_writer, _strings);
        int rootNodeOffset = (int)_writer.Position;

        if (_root._value is not (null or IBymlNode)) {
            throw new InvalidOperationException("""
                Root node must be a container type or null.
                """);
        }

        Write(_root);

        _writer.Seek(0);
        _writer.Write<BymlHeader, BymlHeader.Reverser>(new(
            magic: _writer.Endianness == Endianness.Little
                ? BYML_MAGIC_LE : Byml.BYML_MAGIC,
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
            container.Write(_writer);
            return;
        }

        switch (byml.Type) {
            case BymlNodeType.String:
                break;
            case BymlNodeType.Binary:
                break;
            case BymlNodeType.BinaryAligned:
                break;
            case BymlNodeType.UInt64:
                break;
            default:
                throw new NotImplementedException($"""
                    The type {byml.Type} is not supported by the writer.
                    """);
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
    public void AddNode(in Byml node, int hash) => _referenceNodes[node] = hash;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddKey(string value) => _keys.Add(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddString(string value) => _strings.Add(value);
}
