using System.Runtime.InteropServices;

namespace BymlLibrary.Structures;

[StructLayout(LayoutKind.Explicit, Pack = 4, Size = 4)]
internal struct BymlValue(int value)
{
    // First two fields are
    // just for readability

    [FieldOffset(0)]
    public int Offset;

    [FieldOffset(0)]
    public int Index;

    [FieldOffset(0)]
    public int Int = value;

    [FieldOffset(0)]
    public uint UInt32;

    [FieldOffset(0)]
    public float Float;

    [FieldOffset(0)]
    public bool Bool;
}
