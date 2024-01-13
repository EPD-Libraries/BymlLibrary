using BymlLibrary.Nodes.Containers;
using BymlLibrary.Nodes.Containers.HashMap;

namespace BymlLibrary.Tests.Bogus;

public class BymlGenerator
{
    public static Byml CreateWithEveryType()
    {
        BymlMap root = [];

        // HashMap32
        BymlHashMap32 hashMap32 = new() {
            { uint.MinValue / 2, int.MaxValue / 2 },
            { uint.MaxValue / 2, "HashMap32_Value" },
        };

        root.Add(BymlNodeType.HashMap32.ToString(), hashMap32);

        // HashMap64
        BymlHashMap64 hashMap64 = new() {
            { ulong.MinValue / 2, long.MaxValue / 2 },
            { ulong.MaxValue / 2, "HashMap64_Value" },
        };

        root.Add(BymlNodeType.HashMap64.ToString(), hashMap64);

        // String
        root.Add(BymlNodeType.String.ToString(),
            "String Value");

        // Binary
        root.Add(BymlNodeType.Binary.ToString(),
            "Binary Value"u8.ToArray());

        // BinaryAligned
        root.Add(BymlNodeType.BinaryAligned.ToString(),
            ("Binary Value Ali"u8.ToArray(), 16));

        // Map
        BymlMap map = new() {
            { "MapKey_A", float.Pi },
            { "MapKey_B", "Map_Value" },
        };

        root.Add(BymlNodeType.Map.ToString(), map);

        // Array
        BymlArray array = [
            double.NaN,
            "Array_Value"
        ];

        root.Add(BymlNodeType.Array.ToString(), array);

        // Bool
        root.Add(BymlNodeType.Bool.ToString(), true);

        // Int
        root.Add(BymlNodeType.Int.ToString(), int.MaxValue);

        // Float
        root.Add(BymlNodeType.Float.ToString(), float.Pi);

        // UInt32
        root.Add(BymlNodeType.UInt32.ToString(), uint.MaxValue);

        // Int64
        root.Add(BymlNodeType.Int64.ToString(), long.MaxValue);

        // UInt64
        root.Add(BymlNodeType.UInt64.ToString(), ulong.MaxValue);

        // Double
        root.Add(BymlNodeType.Double.ToString(), double.Pi);

        // Null
        root.Add(BymlNodeType.Null.ToString(), new());

        return root;
    }
}
