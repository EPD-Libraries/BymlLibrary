namespace Nintendo.Byml
{
    /// <summary>
    /// Represents the type of which a dynamic BYML node can be.
    /// </summary>
    public enum NodeType : byte
    {
        None,
        String = 0xA0,
        Binary = 0xA1,
        Array = 0xC0,
        Hash = 0xC1,
        StringArray = 0xC2,
        Bool = 0xD0,
        Int = 0xD1,
        Float = 0xD2,
        UInt = 0xD3,
        Int64 = 0xD4,
        UInt64 = 0xD5,
        Double = 0xD6,
        Null = 0xFF,
    }
}
