using BymlLibrary.Writers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace BymlLibrary.Nodes.Containers.HashMap;

public class BymlHashMap32 : SortedDictionary<uint, Byml>, IBymlNode
{
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
        context.WriteContainerHeader(BymlNodeType.HashMap32, Count);
        foreach ((var key, var node) in this) {
            context.Writer.Write(key);
            write(node);
        }

        foreach (var node in Values) {
            context.Writer.Write(node.Type);
        }

        context.Writer.Align(4);
    }

    internal class ValueEqualityComparer : IEqualityComparer<BymlHashMap32>
    {
        public bool Equals(BymlHashMap32? x, BymlHashMap32? y)
        {
            if (x is null || y is null) {
                return y == x;
            }

            if (x.Count != y.Count) {
                return false;
            }

            return x.Keys.SequenceEqual(y.Keys) && x.Values.SequenceEqual(y.Values, Byml.ValueEqualityComparer.Default);
        }

        public int GetHashCode([DisallowNull] BymlHashMap32 obj)
        {
            throw new NotImplementedException();
        }
    }
}
