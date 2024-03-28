using BymlLibrary.Extensions;
using BymlLibrary.Writers;
using BymlLibrary.Yaml;
using Revrs;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using VYaml.Emitter;

namespace BymlLibrary.Nodes.Containers;

public class BymlMap : SortedDictionary<string, Byml>, IBymlNode
{
    public BymlMap() : base(StringComparer.Ordinal)
    {
    }

    public BymlMap(IDictionary<string, Byml> values) : base(values, StringComparer.Ordinal)
    {
    }

    public void EmitYaml(ref Utf8YamlEmitter emitter)
    {
        emitter.BeginMapping((Count < Byml.YamlConfig.InlineContainerMaxCount && !HasContainerNodes()) switch {
            true => MappingStyle.Flow,
            false => MappingStyle.Block,
        });

        foreach (var (key, node) in this) {
            emitter.WriteString(key);
            BymlYamlWriter.Write(ref emitter, node);
        }

        emitter.EndMapping();
    }

    public int GetValueHash()
    {
        HashCode hashCode = new();
        foreach ((var key, var node) in this) {
            hashCode.Add(key);
            hashCode.Add(Byml.ValueEqualityComparer.Default.GetHashCode(node));
        }

        return hashCode.ToHashCode();
    }

    public bool HasContainerNodes()
    {
        foreach ((_, var node) in this) {
            if (node.Type.IsContainerType()) {
                return true;
            }
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    int IBymlNode.Collect(in BymlWriter writer)
    {
        HashCode hashCode = new();
        foreach ((var key, var node) in this) {
            writer.AddKey(key);
            hashCode.Add(key);
            hashCode.Add(writer.Collect(node));
        }

        return hashCode.ToHashCode();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void IBymlNode.Write(BymlWriter context, Action<Byml> write)
    {
        context.WriteContainerHeader(BymlNodeType.Map, Count);
        foreach ((var key, var node) in this) {
            MapEntryHeader header = new(context.GetKeyIndex(key), node.Type);
            context.Writer.Write<MapEntryHeader, MapEntryHeader.Reverser>(header);
            write(node);
        }
    }

    [StructLayout(LayoutKind.Explicit, Pack = 4, Size = 4)]
    private struct MapEntryHeader(int index, BymlNodeType type)
    {
        [FieldOffset(0)]
        public int Index = index;

        [FieldOffset(3)]
        public BymlNodeType Type = type;

        public class Reverser : IStructReverser
        {
            public static void Reverse(in Span<byte> slice)
            {
                slice[0..3].Reverse();
            }
        }
    }

    public class ValueEqualityComparer : IEqualityComparer<BymlMap>
    {
        public bool Equals(BymlMap? x, BymlMap? y)
        {
            if (x is null || y is null) {
                return y == x;
            }

            if (x.Count != y.Count) {
                return false;
            }

            return x.Keys.SequenceEqual(y.Keys) && x.Values.SequenceEqual(y.Values, Byml.ValueEqualityComparer.Default);
        }

        public int GetHashCode([DisallowNull] BymlMap obj)
        {
            throw new NotImplementedException();
        }
    }
}
