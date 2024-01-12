using System.Runtime.CompilerServices;

namespace BymlLibrary.Extensions;

public static class BymlNodeExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Assert(this BymlNodeType found, BymlNodeType expected)
    {
        if (found != expected) {
            throw new InvalidDataException($"""
                Unexpected node type: {found}.

                Expected {expected} and found {found}.
                """);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsContainerType(this BymlNodeType type)
    {
        return type is
            BymlNodeType.HashMap32 or
            BymlNodeType.HashMap64 or
            BymlNodeType.RelocatedHashMap32 or
            BymlNodeType.RelocatedHashMap64 or
            BymlNodeType.Array or
            BymlNodeType.Map or
            BymlNodeType.StringTable or
            BymlNodeType.RemappedMap or
            BymlNodeType.RelocatedStringTable or
            BymlNodeType.MonoTypedArray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSpecialValueType(this BymlNodeType type)
    {
        return type is
            BymlNodeType.Binary or
            BymlNodeType.BinaryAligned or
            BymlNodeType.Int64 or
            BymlNodeType.UInt64 or
            BymlNodeType.Double;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValueType(this BymlNodeType type)
    {
        return type is
            BymlNodeType.String or
            BymlNodeType.Bool or
            BymlNodeType.Int or
            BymlNodeType.Float or
            BymlNodeType.UInt32 or
            BymlNodeType.Null;
    }
}
