using BymlLibrary.Extensions;
using BymlLibrary.Writers;
using BymlLibrary.Yaml;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using LiteYaml.Emitter;

namespace BymlLibrary.Nodes.Containers.HashMap;

public class BymlHashMap64 : SortedDictionary<ulong, Byml>, IBymlNode
{
    public BymlHashMap64()
    {
    }

    public BymlHashMap64(IDictionary<ulong, Byml> values) : base(values)
    {
    }

    public void EmitYaml(ref Utf8YamlEmitter emitter)
    {
        emitter.SetTag("!h64");
        emitter.BeginMapping((Count < Byml.YamlConfig.InlineContainerMaxCount && !HasContainerNodes()) switch {
            true => MappingStyle.Flow,
            false => MappingStyle.Block,
        });

        foreach (var (hash, node) in this) {
            emitter.WriteUInt64(hash);
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
            hashCode.Add(key);
            hashCode.Add(writer.Collect(node));
        }

        return hashCode.ToHashCode();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void IBymlNode.Write(BymlWriter context, Action<Byml> write)
    {
        context.WriteContainerHeader(BymlNodeType.HashMap64, Count);
        foreach ((var key, var node) in this) {
            context.Writer.Write(key);
            write(node);
        }

        foreach (var node in Values) {
            context.Writer.Write(node.Type);
        }

        context.Writer.Align(4);
    }

    public class ValueEqualityComparer : IEqualityComparer<BymlHashMap64>
    {
        public bool Equals(BymlHashMap64? x, BymlHashMap64? y)
        {
            if (x is null || y is null) {
                return y == x;
            }

            if (x.Count != y.Count) {
                return false;
            }

            return x.Keys.SequenceEqual(y.Keys) && x.Values.SequenceEqual(y.Values, Byml.ValueEqualityComparer.Default);
        }

        public int GetHashCode([DisallowNull] BymlHashMap64 obj)
        {
            throw new NotImplementedException();
        }
    }
}
