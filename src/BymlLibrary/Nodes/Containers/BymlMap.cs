using BymlLibrary.Writers;
using Revrs;

namespace BymlLibrary.Nodes.Containers;

public class BymlMap : Dictionary<string, Byml>, IBymlNode
{
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

    void IBymlNode.Write(RevrsWriter writer)
    {
        throw new NotImplementedException();
    }
}
