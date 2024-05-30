using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BymlLibrary.Extensions;

public static class Utf8Extensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe string ToManaged(this Span<byte> utf8)
    {
        fixed (byte* ptr = utf8) {
            return Marshal.PtrToStringUTF8((IntPtr)ptr, utf8.Length - 1);
        }
    }
}
