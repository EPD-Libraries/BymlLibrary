using BymlLibrary.Structures;
using Revrs;
using Revrs.Extensions;
using System.Runtime.CompilerServices;

namespace BymlLibrary.Nodes.Immutable.Containers;

public readonly ref struct ImmutableBymlStringTable(Span<byte> data, int offset, int count)
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
    private readonly int _count = count;

    /// <summary>
    /// The string offsets in this container
    /// </summary>
    private readonly ReadOnlySpan<int> _offsets
        = data[(offset + BymlContainer.SIZE)..].ReadSpan<int>(++count);

    public readonly Span<byte> this[int index] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            int start = _offset + _offsets[index];
            int end = _offset + _offsets[++index];
            return _data[start..end];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Enumerator GetEnumerator()
        => new(this);

    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref struct Enumerator(ImmutableBymlStringTable stringTable)
    {
        private readonly ImmutableBymlStringTable _container = stringTable;
        private int _index = -1;

        public readonly Span<byte> Current {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _container[_index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            if (++_index >= _container._count) {
                return false;
            }

            return true;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Reverse(ref RevrsReader reader, int offset, int count)
    {
        Span<int> offsets = reader.ReadSpan<int>(++count);
        reader.Seek(offset + offsets[^1]);
    }
}
