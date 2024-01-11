using Revrs;
using System.Runtime.InteropServices;

namespace BymlLibrary.Structures;

[StructLayout(LayoutKind.Explicit, Pack = 2, Size = 0x10)]
public struct BymlHeader(ushort magic, ushort version, int keyTableOffset, int stringTableOffset, int rootNodeOffset)
{
    internal const int SIZE = 0x10;

    [FieldOffset(0x0)]
    public ushort Magic = magic;

    [FieldOffset(0x2)]
    public ushort Version = version;

    [FieldOffset(0x4)]
    public int KeyTableOffset = keyTableOffset;

    [FieldOffset(0x8)]
    public int StringTableOffset = stringTableOffset;

    [FieldOffset(0xC)]
    public int RootNodeOffset = rootNodeOffset;

    public class Reverser : IStructReverser
    {
        public static void Reverse(in Span<byte> slice)
        {
            slice[0x00..0x02].Reverse();
            slice[0x02..0x04].Reverse();
            slice[0x04..0x08].Reverse();
            slice[0x08..0x0C].Reverse();
            slice[0x0C..0x10].Reverse();
        }
    }
}
