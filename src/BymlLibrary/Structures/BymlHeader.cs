using Revrs;
using System.Runtime.InteropServices;

namespace BymlLibrary.Structures;

[StructLayout(LayoutKind.Explicit, Pack = 2, Size = 0x10)]
public struct BymlHeader
{
    [FieldOffset(0x0)]
    public ushort Magic;

    [FieldOffset(0x2)]
    public ushort Version;

    [FieldOffset(0x4)]
    public int KeyTableOffset;

    [FieldOffset(0x8)]
    public int StringTableOffset;

    [FieldOffset(0xC)]
    public int RootNodeOffset;

    public class Reverser : IStructReverser
    {
        public static void Reverse(in Span<byte> slice)
        {
            slice[0x02..0x04].Reverse();
            slice[0x04..0x08].Reverse();
            slice[0x08..0x0C].Reverse();
            slice[0x0C..0x10].Reverse();
        }
    }
}
