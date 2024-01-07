namespace BymlLibrary;

public enum BymlNodeType : byte
{
    // A better solution could be
    // used for handling these map tyes
    HashMap32 = 0x20, // ✔
    HashMap64 = 0x21, // ✔
    RelocatedHashMap32 = 0x30, // Unknown
    RelocatedHashMap64 = 0x31, // Unknown
    String = 0xA0, // ✔
    Binary = 0xA1, // ✔
    BinaryAligned = 0xA2, // ✔
    Array = 0xC0, // ✔
    Map = 0xC1, // ✔
    StringTable = 0xC2, // ✔
    RemappedMap = 0xC4, // Unknown
    RelocatedStringTable = 0xC5, // Unknown
    MonoTypedArray = 0xC8, // Unknown

    // Value Types
    Bool = 0xD0, // ✔
    Int = 0xD1, // ✔
    Float = 0xD2, // ✔
    UInt32 = 0xD3, // ✔
    Int64 = 0xD4, // ✔
    UInt64 = 0xD5, // ✔
    Double = 0xD6, // ✔
    Null = 0xFF, // ✔
}

public class Byml : BymlContainerNode
{
    /// <summary>
    /// <c>BY</c>
    /// </summary>
    internal const ushort BYML_MAGIC = 0x5942;

    /// <summary>
    /// <c>YB</c>
    /// </summary>
    internal const ushort BYML_MAGIC_LE = 0x4259;

    public static BymlContainerNode FromImmutable(ref ImmutableByml byml)
    {
        throw new NotImplementedException();
    }
}
