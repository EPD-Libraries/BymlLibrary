namespace BymlLibrary.Nodes.Immutable.Containers.RelocatedHashMap;

public readonly ref struct ImmutableBymlHashMap32Entry(uint hash, Span<byte> data, int value, BymlNodeType type)
{
    public readonly uint Hash = hash;
    public readonly ImmutableByml Node = new(data, value, type);

    public void Deconstruct(out uint hash, out ImmutableByml node)
    {
        hash = Hash;
        node = Node;
    }
}
