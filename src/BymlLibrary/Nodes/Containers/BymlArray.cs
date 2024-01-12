using BymlLibrary.Writers;
using System.Runtime.CompilerServices;

namespace BymlLibrary.Nodes.Containers;

public class BymlArray : List<Byml>, IBymlNode
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    int IBymlNode.Collect(in BymlWriterContext writer)
    {
        HashCode hashCode = new();
        foreach (var node in this) {
            hashCode.Add(writer.Collect(node));
        }

        return hashCode.ToHashCode();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    int IBymlNode.Write(BymlWriterContext context)
    {
        context.WriteContainerHeader(BymlNodeType.Array, Count);
        foreach (var node in this) {
            context.Writer.Write(node.Type);
        }

        context.Writer.Align(4);

        int staged = 0;
        foreach (var node in this) {
            context.WriteContainerNode(node);
            staged++;
        }

        return staged;
    }
}
