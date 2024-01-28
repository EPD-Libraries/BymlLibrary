using BymlLibrary.Extensions;
using BymlLibrary.Nodes.Containers.HashMap;
using BymlLibrary.Structures;
using BymlLibrary.Yaml;
using Revrs;
using Revrs.Extensions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BymlLibrary.Nodes.Immutable.Containers.HashMap;

public readonly ref struct ImmutableBymlHashMap64(Span<byte> data, int offset, int count, BymlNodeType type)
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
    private readonly int Count = count;

    /// <summary>
    /// The container item count
    /// </summary>
    private readonly BymlNodeType Type = type;

    /// <summary>
    /// Container offset entries
    /// </summary>
    private readonly Span<Entry> _entries = count == 0 ? []
        : data[(offset + BymlContainer.SIZE)..]
            .ReadSpan<Entry>(count);

    /// <summary>
    /// Container offset entries
    /// </summary>
    private readonly Span<BymlNodeType> _types = count == 0 ? []
        : data[(offset + BymlContainer.SIZE + (Entry.SIZE * count))..]
            .ReadSpan<BymlNodeType>(count);

    public readonly ImmutableBymlHashMap64Entry this[int index] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            Entry entry = _entries[index];
            return new(entry.Hash, _data, entry.Value, _types[index]);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = SIZE)]
    private readonly struct Entry
    {
        public const int SIZE = 0xC;

        public readonly ulong Hash;
        public readonly int Value;

        public class Reverser : IStructReverser
        {
            public static void Reverse(in Span<byte> slice)
            {
                slice[0x0..0x8].Reverse();
                slice[0x8..0xC].Reverse();
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Enumerator GetEnumerator()
        => new(this);

    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref struct Enumerator(ImmutableBymlHashMap64 container)
    {
        private readonly ImmutableBymlHashMap64 _container = container;
        private int _index = -1;

        public readonly ImmutableBymlHashMap64Entry Current {
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
    public BymlHashMap64 ToMutable(in ImmutableByml root)
    {
        BymlHashMap64 hashMap64 = [];
        foreach ((var key, var value) in this) {
            hashMap64[key] = Byml.FromImmutable(value, root);
        }

        return hashMap64;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Reverse(ref RevrsReader reader, int offset, int count, in HashSet<int> reversedOffsets)
    {
        for (int i = 0; i < count; i++) {
            Entry entry = reader.Read<Entry, Entry.Reverser>(
                offset + BymlContainer.SIZE + (Entry.SIZE * i)
            );

            ImmutableByml.ReverseNode(ref reader, entry.Value,
                reader.Read<BymlNodeType>(offset + BymlContainer.SIZE + (Entry.SIZE * count) + i),
                reversedOffsets
            );
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal unsafe void EmitYaml(YamlEmitter emitter, in ImmutableByml root)
    {
        emitter.Builder.Append($"!h64");

        if (!emitter.IsIndented && Count < YamlEmitter.FlowContainerStyleMaxChildren && !HasContainerNodes()) {
            emitter.Builder.Append(" {");
            for (int i = 0; i < Count;) {
                var (hash, node) = this[i];
                emitter.Builder.Append($"0x{hash:x16}: ");
                emitter.EmitNode(node, root);
                if (++i < Count) {
                    emitter.Builder.Append(", ");
                }
            }

            emitter.Builder.Append('}');
            return;
        }

        emitter.NewLine();
        foreach ((var hash, var node) in this) {
            if (!emitter.IsIndented) {
                emitter.NewLine();
            }

            emitter.IndentLine();
            emitter.Builder.Append($"0x{hash:x16}");
            emitter.Builder.Append(": ");
            emitter.IsInline = true;
            emitter.IsIndented = false;
            emitter.Level++;
            emitter.EmitNode(node, root);
            emitter.Level--;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HasContainerNodes()
    {
        foreach ((_, var node) in this) {
            if (node.Type.IsContainerType()) {
                return true;
            }
        }

        return false;
    }
}
