using BymlLibrary.Writers;
using System.Runtime.CompilerServices;

namespace BymlLibrary.Nodes.Containers.HashMap;

public class BymlHashMap32 : SortedDictionary<uint, Byml>, IBymlNode
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    int IBymlNode.Collect(in BymlWriterContext writer)
    {
        HashCode hashCode = new();
        foreach ((var key, var node) in this) {
            hashCode.Add(key);
            hashCode.Add(writer.Collect(node));
        }

        return hashCode.ToHashCode();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    int IBymlNode.Write(BymlWriterContext context)
    {
        int staged = 0;
        context.WriteContainerHeader(BymlNodeType.Map, Count);
        foreach ((var key, var node) in this) {
            context.Writer.Write(key);
            context.WriteContainerNode(node);
            staged++;
        }

        foreach (var node in Values) {
            context.Writer.Write(node.Type);
        }

        context.Writer.Align(4);
        return staged;
    }
}
