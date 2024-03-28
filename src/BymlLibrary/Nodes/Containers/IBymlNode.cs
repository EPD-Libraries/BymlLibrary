using BymlLibrary.Writers;
using VYaml.Emitter;

namespace BymlLibrary.Nodes.Containers;

public interface IBymlNode
{
    public int GetValueHash();
    void EmitYaml(ref Utf8YamlEmitter emitter);
    internal int Collect(in BymlWriter writer);
    internal void Write(BymlWriter context, Action<Byml> write);
}
