using Revrs;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BymlLibrary.Structures;

[StructLayout(LayoutKind.Explicit, Pack = 0, Size = SIZE)]
public struct BymlContainerNodeHeader
{
    internal const int SIZE = 4;

    [FieldOffset(0)]
    public BymlNodeType Type;

    [FieldOffset(0)]
    private readonly int _count;

    public readonly int Count {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _count >> 8;
    }

    public class Reverser : IStructReverser
    {
        public static void Reverse(in Span<byte> slice)
        {
            slice[0x1..0x4].Reverse();
        }
    }
}
