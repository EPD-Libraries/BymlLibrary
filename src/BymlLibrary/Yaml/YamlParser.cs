using BymlLibrary.Nodes.Containers;
using BymlLibrary.Nodes.Containers.HashMap;
using System.Runtime.CompilerServices;
using YamlDotNet.Helpers;
using YamlDotNet.RepresentationModel;

namespace BymlLibrary.Yaml;

internal class YamlParser
{
    public static Byml Parse(string text)
    {
        StringReader reader = new(text);
        YamlStream yaml = [];
        yaml.Load(reader);

        if (yaml.Documents.FirstOrDefault() is YamlDocument document) {
            return Parse(document.RootNode);
        }

        throw new InvalidDataException("""
            No yaml documents could be found in the provided text
            """);
    }

    private static Byml Parse(YamlNode node)
    {
        return node.NodeType switch {
            YamlNodeType.Mapping => ParseMap((YamlMappingNode)node),
            YamlNodeType.Sequence => ParseSequence((YamlSequenceNode)node),
            YamlNodeType.Scalar => ParseScalar((YamlScalarNode)node),
            _ => throw new NotSupportedException($"""
                The YamlNodeType '{node.NodeType}' is not supported
                """)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static Byml ParseScalar(YamlScalarNode scalar)
    {
        if (scalar.Value is null) {
            return new();
        }

        if (scalar.Value.AsSpan().IsEmpty) {
            return new(string.Empty);
        }

        if (scalar.Tag.IsEmpty) {
            if (int.TryParse(scalar.Value, out int s32Value)) {
                return s32Value;
            }

            if (float.TryParse(scalar.Value, out float f32Value)) {
                return f32Value;
            }

            if (scalar.Value.Equals("null", StringComparison.CurrentCultureIgnoreCase)) {
                return new();
            }

            bool isTrue = scalar.Value.Equals("true", StringComparison.CurrentCultureIgnoreCase);
            if (isTrue || scalar.Value.Equals("false", StringComparison.CurrentCultureIgnoreCase)) {
                return isTrue;
            }

            return scalar.Value;
        }

        return scalar.Tag.Value switch {
            "!u" or "!u32" => Convert.ToUInt32(scalar.Value[2..], 16),
            "!ul" or "!u64" => Convert.ToUInt64(scalar.Value[2..], 16),
            "!l" or "!s64" => long.Parse(scalar.Value),
            "!d" or "!f64" => double.Parse(scalar.Value),
            "!!binary" or "tag:yaml.org,2002:binary" => Convert.FromBase64String(scalar.Value),
            _ => throw new NotSupportedException($"""
                Unsupported tag '{scalar.Tag.Value}'
                """)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static Byml ParseSequence(YamlSequenceNode sequence)
    {
        BymlArray array = [];
        foreach (var node in sequence.Children) {
            array.Add(Parse(node));
        }

        return array;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static Byml ParseMap(YamlMappingNode mapping)
    {
        if (mapping.Tag.IsEmpty) {
            BymlMap map = [];
            foreach ((var key, var node) in mapping.Children) {
                if (key is not YamlScalarNode scalar) {
                    throw new InvalidOperationException($"""
                        Could not parse key node of type '{key.NodeType}'
                        """);
                }

                if (string.IsNullOrEmpty(scalar.Value)) {
                    throw new NotSupportedException("""
                        Empty (null) keys are not supported
                        """);
                }

                map[scalar.Value] = Parse(node);
            }

            return map;
        }

        return mapping.Tag.Value switch {
            "!h32" => ParseHashMap32(mapping.Children),
            "!h64" => ParseHashMap64(mapping.Children),
            "!!file" or "tag:yaml.org,2002:file" => ParseFile(mapping.Children),
            _ => throw new NotSupportedException($"""
                Unsupported mapping tag '{mapping.Tag.Value}'
                """)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static Byml ParseHashMap32(in IOrderedDictionary<YamlNode, YamlNode> nodes)
    {
        BymlHashMap32 map = [];
        foreach ((var key, var node) in nodes) {
            if (key is not YamlScalarNode scalar) {
                throw new InvalidOperationException($"""
                    Could not parse key node of type '{key.NodeType}'
                    """);
            }

            if (string.IsNullOrEmpty(scalar.Value)) {
                throw new NotSupportedException("""
                    Empty (null) keys are not supported
                    """);
            }

            map[Convert.ToUInt32(scalar.Value[2..], 16)] = Parse(node);
        }

        return map;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static Byml ParseHashMap64(in IOrderedDictionary<YamlNode, YamlNode> nodes)
    {
        BymlHashMap64 map = [];
        foreach ((var key, var node) in nodes) {
            if (key is not YamlScalarNode scalar) {
                throw new InvalidOperationException($"""
                    Could not parse key node of type '{key.NodeType}'
                    """);
            }

            if (string.IsNullOrEmpty(scalar.Value)) {
                throw new NotSupportedException("""
                    Empty (null) keys are not supported
                    """);
            }

            map[Convert.ToUInt64(scalar.Value[2..], 16)] = Parse(node);
        }

        return map;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static Byml ParseFile(in IOrderedDictionary<YamlNode, YamlNode> nodes)
    {
        if (!nodes.TryGetValue("Alignment", out YamlNode? alignmentNode) || !nodes.TryGetValue("Data", out YamlNode? dataNode)) {
            throw new InvalidDataException("""
                Invalid !!file map, could not find Alignment and/or Data
                """);
        }

        if (alignmentNode is not YamlScalarNode alignmentScalarNode || dataNode is not YamlScalarNode dataScalarNode) {
            throw new InvalidDataException("""
                Invalid !!file map, Alignment and/or Data was not a valid scalar node
                """);
        }

        if (alignmentScalarNode.Value is null || dataScalarNode.Value is null) {
            throw new InvalidDataException("""
                Invalid !!file map, Alignment and/or Data was null
                """);
        }

        return (Convert.FromBase64String(dataScalarNode.Value), int.Parse(alignmentScalarNode.Value));
    }
}
