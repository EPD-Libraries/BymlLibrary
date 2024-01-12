using BymlLibrary.Writers;
using System.Runtime.CompilerServices;

namespace BymlLibrary.Nodes.Containers;

public class BymlMap : Dictionary<string, Byml>, IBymlNode
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    int IBymlNode.Collect(in BymlWriterContext writer)
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
    int IBymlNode.Write(BymlWriterContext context)
    {
        int staged = 0;
        context.WriteContainerHeader(BymlNodeType.Map, Count);
        foreach ((var key, var node) in this) {
            int keyIndexAndType = context.Keys.IndexOf(key) | ((byte)node.Type << 24);
            context.Writer.Write(keyIndexAndType);
            context.WriteContainerNode(node);
            staged++;
        }

        return staged;
    }
}
