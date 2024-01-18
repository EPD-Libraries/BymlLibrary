using BymlLibrary.Writers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace BymlLibrary.Nodes.Containers;

public class BymlArray : List<Byml>, IBymlNode
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    int IBymlNode.Collect(in BymlWriter writer)
    {
        HashCode hashCode = new();
        foreach (var node in this) {
            hashCode.Add(writer.Collect(node));
        }

        return hashCode.ToHashCode();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void IBymlNode.Write(BymlWriter context, Action<Byml> write)
    {
        context.WriteContainerHeader(BymlNodeType.Array, Count);
        foreach (var node in this) {
            context.Writer.Write(node.Type);
        }

        context.Writer.Align(4);

        foreach (var node in this) {
            write(node);
        }
    }

    internal class ValueEqualityComparer : IEqualityComparer<BymlArray>
    {
        public bool Equals(BymlArray? x, BymlArray? y)
        {
            if (x is null || y is null) {
                return y == x;
            }

            if (x.Count != y.Count) {
                return false;
            }

            return x.SequenceEqual(y, Byml.ValueEqualityComparer.Default);
        }

        public int GetHashCode([DisallowNull] BymlArray obj)
        {
            throw new NotImplementedException();
        }
    }
}
