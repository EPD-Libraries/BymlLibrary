namespace BymlLibrary.Nodes.Immutable.Containers;

public readonly ref struct ImmutableBymlHashMap64Entry(ulong hash, Span<byte> data, int value, BymlNodeType type)
{
    public readonly ulong Hash = hash;
    public readonly ImmutableByml Node = new(data, value, type);

    public void Deconstruct(out ulong hash, out ImmutableByml node)
    {
        hash = Hash;
        node = Node;
    }
}
