namespace BymlLibrary.Nodes.Immutable.Containers;

public readonly ref struct ImmutableBymlMapEntry(int keyIndex, Span<byte> data, int value, BymlNodeType type)
{
    public readonly int KeyIndex = keyIndex;
    public readonly ImmutableByml Node = new(data, value, type);

    public void Deconstruct(out int keyIndex, out ImmutableByml node)
    {
        keyIndex = KeyIndex;
        node = Node;
    }
}
