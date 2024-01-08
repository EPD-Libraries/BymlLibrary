using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;

namespace BymlLibrary.Extensions;

public static class Utf8Extensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe string ToManaged(this Span<byte> utf8)
    {
        fixed (byte* ptr = utf8) {
            return Utf8StringMarshaller.ConvertToManaged(ptr)
                ?? string.Empty;
        }
    }
}
