using BymlLibrary.Extensions;
using BymlLibrary.Nodes.Containers.HashMap;
using BymlLibrary.Structures;
using BymlLibrary.Yaml;
using Revrs;
using Revrs.Extensions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using VYaml.Emitter;

namespace BymlLibrary.Nodes.Immutable.Containers.HashMap;

public readonly ref struct ImmutableBymlHashMap32(Span<byte> data, int offset, int count)
{
    /// <summary>
    /// Span of the BYMl data
    /// </summary>
    private readonly Span<byte> _data = data;

    /// <summary>
    /// The container item count
    /// </summary>
    public readonly int Count = count;

    /// <summary>
    /// Container offset entries
    /// </summary>
    private readonly Span<Entry> _entries = count == 0 ? []
        : data[(offset + BymlContainer.SIZE)..]
            .ReadSpan<Entry>(count);

    /// <summary>
    /// Container entry types
    /// </summary>
    private readonly Span<BymlNodeType> _types = count == 0 ? []
        : data[(offset + BymlContainer.SIZE + (Entry.SIZE * count))..]
            .ReadSpan<BymlNodeType>(count);

    public readonly ImmutableBymlHashMap32Entry this[int index] {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            Entry entry = _entries[index];
            return new(entry.Hash, _data, entry.Value, _types[index]);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = SIZE)]
    private readonly struct Entry
    {
        public const int SIZE = 8;

        public readonly uint Hash;
        public readonly int Value;

        public class Reverser : IStructReverser
        {
            public static void Reverse(in Span<byte> slice)
            {
                slice[0..4].Reverse();
                slice[4..8].Reverse();
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Enumerator GetEnumerator()
        => new(this);

    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref struct Enumerator(ImmutableBymlHashMap32 container)
    {
        private readonly ImmutableBymlHashMap32 _container = container;
        private int _index = -1;

        public readonly ImmutableBymlHashMap32Entry Current {
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
    public BymlHashMap32 ToMutable(in ImmutableByml root)
    {
        BymlHashMap32 hashMap32 = [];
        foreach ((var key, var value) in this) {
            hashMap32[key] = Byml.FromImmutable(value, root);
        }

        return hashMap32;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal unsafe void EmitYaml(ref Utf8YamlEmitter emitter, in ImmutableByml root)
    {
        emitter.Tag("!h32");
        emitter.BeginMapping((Count < Byml.YamlConfig.InlineContainerMaxCount && !HasContainerNodes()) switch {
            true => MappingStyle.Flow,
            false => MappingStyle.Block,
        });

        foreach (var (hash, node) in this) {
            emitter.WriteUInt32(hash);
            BymlYamlWriter.Write(ref emitter, node, root);
        }

        emitter.EndMapping();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HasContainerNodes()
    {
        foreach (var (_, node) in this) {
            if (node.Type.IsContainerType()) {
                return true;
            }
        }

        return false;
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
}
