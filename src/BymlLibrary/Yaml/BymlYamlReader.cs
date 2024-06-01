using BymlLibrary.Nodes.Containers;
using BymlLibrary.Nodes.Containers.HashMap;
using System.Buffers;
using System.Runtime.CompilerServices;
using LiteYaml.Parser;

namespace BymlLibrary.Yaml;

internal class BymlYamlReader
{
    public static Byml Parse(ArraySegment<byte> data)
    {
        YamlParser parser = new(new ReadOnlySequence<byte>(data));
        parser.SkipAfter(ParseEventType.DocumentStart);
        return Parse(ref parser);
    }

    private static Byml Parse(ref YamlParser parser)
    {
        return parser.CurrentEventType switch {
            ParseEventType.MappingStart => ParseMap(ref parser),
            ParseEventType.SequenceStart => ParseSequence(ref parser),
            ParseEventType.Scalar => ParseScalar(ref parser),
            _ => throw new NotSupportedException($"""
                The event type '{parser.CurrentEventType}' is not supported
                """)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static Byml ParseScalar(ref YamlParser parser)
    {
        if (parser.IsNullScalar()) {
            // Check for empty strings
            if (parser.TryReadScalarAsString(out string? empty) && empty == string.Empty) {
                return string.Empty;
            }

            return new();
        }

        if (parser.TryGetCurrentTag(out Tag tag)) {
            return tag.Suffix switch {
                "s" or "s32" => parser.ReadScalarAsInt32(),
                "u" or "u32" => parser.ReadScalarAsUInt32(),
                "l" or "s64" => parser.ReadScalarAsInt64(),
                "ul" or "u64" => parser.ReadScalarAsUInt64(),
                "f" or "f32" => parser.ReadScalarAsFloat(),
                "d" or "f64" => parser.ReadScalarAsDouble(),
                "binary" or "tag:yaml.org,2002:binary" => Convert.FromBase64String(parser.ReadScalarAsString()
                    ?? throw new InvalidDataException("""
                        Invalid binary data, expected a base64 string
                        """)),
                _ => throw new InvalidDataException($"""
                    Unexpected YAML scalar tag '{tag}'
                    """)
            };
        }

        if (parser.TryReadScalarAsBool(out bool boolean)) {
            return boolean;
        }

        if (parser.TryReadScalarAsInt32(out int int32)) {
            return int32;
        }

        if (parser.TryReadScalarAsUInt32(out uint uint32)) {
            return uint32;
        }

        if (parser.TryReadScalarAsInt64(out long int64)) {
            return int64;
        }

        if (parser.TryReadScalarAsUInt64(out ulong uint64)) {
            return uint64;
        }

        if (parser.TryReadScalarAsFloat(out float float32)) {
            return float32;
        }

        if (parser.TryReadScalarAsDouble(out double float64)) {
            return float64;
        }

        if (parser.TryReadScalarAsString(out string? str)) {
            return str ?? string.Empty;
        }

        return new();
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static Byml ParseSequence(ref YamlParser parser)
    {
        BymlArray array = [];
        parser.SkipAfter(ParseEventType.SequenceStart);

        while (parser.CurrentEventType is not ParseEventType.SequenceEnd) {
            array.Add(Parse(ref parser));
        }

        parser.SkipAfter(ParseEventType.SequenceEnd);
        return array;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static Byml ParseMap(ref YamlParser parser)
    {
        if (parser.TryGetCurrentTag(out var tag)) {
            return tag.Suffix switch {
                "h32" => ParseHashMap32(ref parser),
                "h64" => ParseHashMap64(ref parser),
                "file" => ParseFile(ref parser),
                _ => throw new InvalidDataException($"""
                    Unexpected YAML map tag '{tag}' (expected '!h32', '!h64' or '!!file')
                    """),
            };
        }

        BymlMap map = [];
        parser.SkipAfter(ParseEventType.MappingStart);

        while (parser.CurrentEventType is not ParseEventType.MappingEnd) {
            string key = parser.ReadScalarAsString() ?? string.Empty;
            map[key] = Parse(ref parser);
        }

        parser.SkipAfter(ParseEventType.MappingEnd);
        return map;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static Byml ParseHashMap32(ref YamlParser parser)
    {
        BymlHashMap32 map = [];
        parser.SkipAfter(ParseEventType.MappingStart);

        while (parser.CurrentEventType is not ParseEventType.MappingEnd) {
            map[parser.ReadScalarAsUInt32()] = Parse(ref parser);
        }

        parser.SkipAfter(ParseEventType.MappingEnd);
        return map;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static Byml ParseHashMap64(ref YamlParser parser)
    {
        BymlHashMap64 map = [];
        parser.SkipAfter(ParseEventType.MappingStart);

        while (parser.CurrentEventType is not ParseEventType.MappingEnd) {
            map[parser.ReadScalarAsUInt64()] = Parse(ref parser);
        }

        parser.SkipAfter(ParseEventType.MappingEnd);
        return map;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static Byml ParseFile(ref YamlParser parser)
    {
        parser.SkipAfter(ParseEventType.MappingStart);

        parser.SkipCurrentNode();
        int alignment = parser.ReadScalarAsInt32();
        parser.SkipCurrentNode();
        string base64 = parser.ReadScalarAsString()
            ?? throw new InvalidDataException("""
                Invalid binary data, expected a base64 string
                """);

        parser.SkipAfter(ParseEventType.MappingEnd);
        return (Convert.FromBase64String(base64), alignment);
    }
}
