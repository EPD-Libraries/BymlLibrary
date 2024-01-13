using BymlLibrary.Writers;
using System.Runtime.CompilerServices;

namespace BymlLibrary.Nodes.Containers.HashMap;

public class BymlHashMap64 : SortedDictionary<ulong, Byml>, IBymlNode
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
}
