using BymlLibrary.Writers;
using Revrs;

namespace BymlLibrary.Nodes.Containers;

public class BymlArray : List<Byml>, IBymlNode
{
    int IBymlNode.Collect(in BymlWriter writer)
    {
        HashCode hashCode = new();
        foreach (var node in this) {
            hashCode.Add(writer.Collect(node));
        }

        return hashCode.ToHashCode();
    }

    void IBymlNode.Write(RevrsWriter writer)
    {
        throw new NotImplementedException();
    }
}
