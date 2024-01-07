using System.Runtime.CompilerServices;

namespace BymlLibrary.Extensions;

public static class BymlNodeExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Assert(this BymlNodeType found, BymlNodeType expected)
    {
        if (found != expected) {
            if (expected == BymlNodeType.HashMap && found.IsHasMap()) {
                return;
            }

            if (expected == BymlNodeType.RemappedHashMap && found.IsRemappedHasMap()) {
                return;
            }

            throw new InvalidDataException($"""
                Unexpected node type: {found}.

                Expected {expected} and found {found}.
                """);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Assert(this BymlNodeType found, BymlNodeType[] expected)
    {
        if (!expected.Contains(found)) {
            throw new InvalidDataException($"""
                Unexpected node type: {found}.

                Expected one of {string.Join(", ", expected)} and found {found}.
                """);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsContainerType(this BymlNodeType type)
    {
        return
            type.IsHasMap() ||
            type.IsRemappedHasMap() ||
            type is
            BymlNodeType.Array or
            BymlNodeType.Map or
            BymlNodeType.StringTable or
            BymlNodeType.RemappedMap or
            BymlNodeType.RelocatedStringTable or
            BymlNodeType.MonoTypedArray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsHasMap(this BymlNodeType type)
    {
        return type >= BymlNodeType.HashMap
            && (byte)type <= 0x2F;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRemappedHasMap(this BymlNodeType type)
    {
        return type >= BymlNodeType.RemappedHashMap
            && (byte)type <= 0x3F;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSpecialValueType(this BymlNodeType type)
    {
        return type is
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
