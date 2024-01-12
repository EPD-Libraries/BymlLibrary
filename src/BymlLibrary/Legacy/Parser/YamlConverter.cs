#pragma warning disable CS8602, CS8603, CS8604

using System.Diagnostics;
using System.Globalization;
using System.Text;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;

namespace BymlLibrary.Legacy.Parser;

[Obsolete("BymlFile is obsolete, use BymlLibrary.Byml")]
public class YamlConverter
{
    private static Dictionary<string, BymlNode> ReferenceNodes { get; set; } = [];

    public static string ToYaml(BymlFile byml)
    {
        YamlNode root = SaveNode(byml.RootNode);
        YamlStream stream = new(new YamlDocument(root));
        string ret;
        using (StringWriter writer = new(new StringBuilder())) {
            stream.Save(writer, true);
            ret = writer.ToString();
        }
        return ret;
    }

    public static string ToYaml(BymlNode byml)
    {
        YamlNode root = SaveNode(byml);
        YamlStream stream = new(new YamlDocument(root));
        string ret;
        using (StringWriter writer = new(new StringBuilder())) {
            stream.Save(writer, true);
            ret = writer.ToString();
        }
        return ret;
    }

    public static BymlFile FromYaml(string text)
    {
        ReferenceNodes.Clear();

        var byml = new BymlFile();
        var yaml = new YamlStream();

        yaml.Load(new StringReader(text));

        YamlNode root = yaml.Documents[0].RootNode;

        if (root is YamlMappingNode || root is YamlSequenceNode) {
            byml.RootNode = ParseNode(root);
        }

        ReferenceNodes.Clear();

        return byml;
    }

    static BymlNode ParseNode(YamlNode node)
    {
        if (node is YamlMappingNode castMappingNode) {
            var values = new Dictionary<string, BymlNode>();
            if (IsValidReference(node)) {
                ReferenceNodes.Add(node.Tag.Value, new BymlNode(values));
            }

            foreach (var child in castMappingNode.Children) {
                var key = ((YamlScalarNode)child.Key).Value;
                var tag = ((YamlScalarNode)child.Key).Tag;
                if (tag == "!h") {
                    key = Crc32.Compute(key).ToString("x");
                }

                values.Add(key, ParseNode(child.Value));
            }
            return new BymlNode(values);
        }
        else if (node is YamlSequenceNode castSequenceNode) {

            var values = new List<BymlNode>();
            if (IsValidReference(node)) {
                ReferenceNodes.Add(node.Tag.Value, new BymlNode(values));
            }

            foreach (var child in castSequenceNode.Children) {
                values.Add(ParseNode(child));
            }

            return new BymlNode(values);
        }
        else if (node is YamlScalarNode castScalarNode && castScalarNode.Value.Contains("!refTag=")) {

            string tag = castScalarNode.Value.Replace("!refTag=", string.Empty);
            Debug.WriteLine($"refNode {tag} {ReferenceNodes.ContainsKey(tag)}");

            if (ReferenceNodes.TryGetValue(tag, out BymlNode? value)) {
                return value;
            }
            else {
                Console.WriteLine("Failed to find reference node! " + tag);
                return null;
            }
        }
        else {
            return ConvertValue(((YamlScalarNode)node).Value, ((YamlScalarNode)node).Tag.Value);
        }
    }

    static bool IsValidReference(YamlNode node)
    {
        return !node.Tag.IsEmpty && node.Tag.Value.Contains("!ref") && !ReferenceNodes.ContainsKey(node.Tag.Value);
    }

    static BymlNode ConvertValue(string value, string tag)
    {
        tag ??= "";

        if (value == "null") {
            return new BymlNode();
        }
        else if (value is "true" or "True") {
            return new BymlNode(true);
        }
        else if (value is "false" or "False") {
            return new BymlNode(false);
        }
        else if (tag == "!u") {
            return new BymlNode(
                Convert.ToUInt32(value[2..], 16)
            );
        }
        else if (tag == "!d") {
            return new BymlNode(double.Parse(value, CultureInfo.InvariantCulture));
        }
        else if (tag == "!ul") {
            return new BymlNode(
                Convert.ToUInt64(value[2..], 16)
            );
        }
        else if (tag == "!l") {
            return new BymlNode(long.Parse(value, CultureInfo.InvariantCulture));
        }
        else if (tag == "!h") {
            return new BymlNode(Crc32.Compute(value).ToString("x"));
        }
        else {
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue)) {
                return new BymlNode(intValue);
            }
            if (float.TryParse(value, out float floatValue)) {
                return new BymlNode(floatValue);
            }
        }

        return new BymlNode(value != "''" ? value : string.Empty);
    }

    static YamlNode SaveNode(BymlNode node)
    {
        if (node is null) {
            return new YamlScalarNode("null");
        }
        else if (node.Type == NodeType.Array) {
            var yamlNode = new YamlSequenceNode();

            if (node.Array.Count < 6 && !HasEnumerables(node)) {
                yamlNode.Style = SequenceStyle.Flow;
            }

            foreach (BymlNode item in node.Array) {
                yamlNode.Add(SaveNode(item));
            }

            return yamlNode;
        }
        else if (node.Type == NodeType.Hash) {
            var yamlNode = new YamlMappingNode();

            if (node.Hash.Count < 6 && !HasEnumerables(node)) {
                yamlNode.Style = MappingStyle.Flow;
            }

            foreach ((string key, BymlNode item) in node.Hash) {
                YamlScalarNode keyNode = new(key);
                if (IsHash(key)) {
                    uint hash = Convert.ToUInt32(key, 16);
                    if (Hashes.TryGetValue(hash, out string? value)) {
                        keyNode.Value = value;
                    }
                }
                yamlNode.Add(keyNode, SaveNode(item));
            }
            return yamlNode;
        }
        else {
            var yamlNode = new YamlScalarNode(ConvertValue(node)) {
                Tag = node.Type switch {
                    NodeType.UInt => "!u",
                    NodeType.Int64 => "!l",
                    NodeType.UInt64 => "!ul",
                    NodeType.Double => "!d",
                    _ => null,
                }
            };
            return yamlNode;
        }
    }

    private static bool HasEnumerables(BymlNode node)
    {
        return node.Type switch {
            NodeType.Array => node.Array.Any(n => n.Type == NodeType.Array || n.Type == NodeType.Hash),
            NodeType.Hash => node.Hash.Any(p => p.Value.Type == NodeType.Array || p.Value.Type == NodeType.Hash),
            _ => false,
        };
    }

    private static string ConvertValue(BymlNode node)
    {
        return node.Type switch {
            NodeType.String => !string.IsNullOrEmpty(node.String) ? node.String : "''",
            NodeType.Bool => node.Bool ? "true" : "false",
            NodeType.Binary => string.Join(" ", node.Binary),
            NodeType.Int => node.Int.ToString(CultureInfo.InvariantCulture),
            NodeType.Float => FormatFloat(node.Float),
            NodeType.UInt => $"0x{node.UInt:x8}",
            NodeType.Int64 => node.Int64.ToString(CultureInfo.InvariantCulture),
            NodeType.UInt64 => $"0x{node.UInt64:x16}",
            NodeType.Double => FormatDouble(node.Double),
            _ => throw new ArgumentException($"Not value node type {node.Type}"),
        };
    }

    private static Dictionary<uint, string> Hashes => CreateHashList();
    private static Dictionary<uint, string> CreateHashList()
    {
        List<string> hashLists =
        [
            "AcnhByml",
            "AcnhHeaders",
            "AcnhValues"
        ];

        Dictionary<uint, string> hashes = [];

        foreach (var list in hashLists) {
            string hashList = new Resource($"Legacy.Data.{list}").ToString();
            foreach (string hashStr in hashList.Split('\n')) {
                CheckHash(ref hashes, hashStr);
            }
        }

        return hashes;
    }

    private static void CheckHash(ref Dictionary<uint, string> hashes, string hashStr)
    {
        uint hash = Crc32.Compute(hashStr);
        hashes.TryAdd(hash, hashStr);
    }

    public static bool IsHash(string k) => k != null && IsHex(k.ToArray());
    private static bool IsHex(IEnumerable<char> chars)
    {
        bool isHex;

        foreach (var c in chars) {
            isHex = (c >= '0' && c <= '9') ||
                     (c >= 'a' && c <= 'f') ||
                     (c >= 'A' && c <= 'F');

            if (!isHex) {
                return false;
            }
        }

        return true;
    }

    private static string FormatFloat(float f) => $"{f:0.0########}";
    private static string FormatDouble(double d) => $"{d:0.0########}";
}
