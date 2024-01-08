using BymlLibrary.Extensions;
using BymlLibrary.Structures;
using BymlLibrary.Yaml;
using Revrs;
using Revrs.Extensions;
using System.Runtime.CompilerServices;

namespace BymlLibrary.Nodes.Immutable.Containers;

public readonly ref struct ImmutableBymlArray(Span<byte> data, int offset, int count)
{
    /// <summary>
    /// Span of the BYMl data
    /// </summary>
    private readonly Span<byte> _data = data;

    /// <summary>
    /// The container offset (start of header)
    /// </summary>
    private readonly int _offset = offset;

    /// <summary>
    /// The container item count
    /// </summary>
    public readonly int Count = count;

    /// <summary>
    /// Container types
    /// </summary>
    private readonly Span<BymlNodeType> _types = count == 0 ? []
        : data[(offset + BymlContainerNodeHeader.SIZE)..]
            .ReadSpan<BymlNodeType>(count);

    /// <summary>
    /// Container values
    /// </summary>
    private readonly Span<int> _values = count == 0 ? []
        : data[(offset + BymlContainerNodeHeader.SIZE + count + count.AlignUp(4))..]
            .ReadSpan<int>(count);

    public readonly ImmutableByml this[int index] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            return new(_data, _values[index], _types[index]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Enumerator GetEnumerator()
        => new(this);

    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref struct Enumerator(ImmutableBymlArray container)
    {
        private readonly ImmutableBymlArray _container = container;
        private int _index = -1;

        public readonly ImmutableByml Current {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _container[_index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            if (++_index >= _container.Count) {
                return false;
            }

            return true;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public List<Byml> ToMutable(in ImmutableByml root)
    {
        List<Byml> array = [];
        foreach (var value in this) {
            array.Add(Byml.FromImmutable(value, root));
        }

        return array;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Reverse(ref RevrsReader reader, int offset, int count, in HashSet<int> reversedOffsets)
    {
        reader.Seek(offset + BymlContainerNodeHeader.SIZE);
        Span<BymlNodeType> types = reader.ReadSpan<BymlNodeType>(count);
        reader.Align(4);
        Span<int> values = reader.ReadSpan<int>(count);

        for (int i = 0; i < count; i++) {
            ImmutableByml.ReverseNode(ref reader, values[i], types[i], reversedOffsets);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EmitYaml(YamlEmitter emitter, in ImmutableByml root)
    {
        if (Count <= 5 && !HasContainerNodes()) {
            emitter.Builder.Append('[');
            for (int i = 0; i < Count;) {
                emitter.EmitNode(this[i], root);
                if (++i < Count) {
                    emitter.Builder.Append(", ");
                }
            }

            emitter.Builder.Append(']');
            return;
        }

        foreach (var node in this) {
            emitter.NewLine();
            emitter.IndentLine();
            emitter.Builder.Append("- ");
            emitter.IsIndented = true;
            emitter.Level++;
            emitter.EmitNode(node, root);
            emitter.Level--;
            emitter.IsIndented = false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasContainerNodes()
    {
        foreach (var node in this) {
            if (node.Type.IsContainerType()) {
                return true;
            }
        }

        return false;
    }
}
