#pragma warning disable CS8600, CS8602, CS8604, CS8618

using BymlLibrary.Legacy.Parser;

namespace BymlLibrary.Legacy;


[Obsolete("BymlFile is obsolete, use BymlLibrary.Byml")]
public class BymlNode : IEquatable<BymlNode>
{
    private readonly NodeType type;
    private readonly List<BymlNode> array;
    private readonly SortedDictionary<string, BymlNode> dict;
    private readonly List<string> str_array;
    private BymlUnion union;
    public NodeType Type { get => type; }
    public List<BymlNode> Array { get => array; }
    public SortedDictionary<string, BymlNode> Hash { get => dict; }
    public List<string> StringArray { get => str_array; }
    public string String { get => union.String; set => union.String = value; }
    public byte[] Binary { get => union.Binary; set => union.Binary = value; }
    public bool Bool { get => union.Bool; set => union.Bool = value; }
    public int Int { get => union.Int; set => union.Int = value; }
    public float Float { get => union.Float; set => union.Float = value; }
    public uint UInt { get => union.UInt; set => union.UInt = value; }
    public long Int64 { get => union.Int64; set => union.Int64 = value; }
    public ulong UInt64 { get => union.UInt64; set => union.UInt64 = value; }
    public double Double { get => union.Double; set => union.Double = value; }

    public BymlNode()
    {
        type = NodeType.Null;
    }
    public BymlNode(string str)
    {
        union.String = str;
        type = NodeType.String;
    }
    public BymlNode(byte[] bytes)
    {
        union.Binary = bytes;
        type = NodeType.Binary;
    }
    public BymlNode(List<BymlNode> array)
    {
        this.array = array;
        type = NodeType.Array;
    }
    public BymlNode(SortedDictionary<string, BymlNode> dict)
    {
        this.dict = dict;
        type = NodeType.Hash;
    }
    public BymlNode(Dictionary<string, BymlNode> dict)
    {
        this.dict = new SortedDictionary<string, BymlNode>(dict, new AsciiComparer());
        type = NodeType.Hash;
    }
    public BymlNode(List<string> str_array)
    {
        this.str_array = str_array;
        type = NodeType.StringArray;
    }
    public BymlNode(bool b)
    {
        union.Bool = b;
        type = NodeType.Bool;
    }
    public BymlNode(int i)
    {
        union.Int = i;
        type = NodeType.Int;
    }
    public BymlNode(float f)
    {
        union.Float = f;
        type = NodeType.Float;
    }
    public BymlNode(uint u)
    {
        union.UInt = u;
        type = NodeType.UInt;
    }
    public BymlNode(long l)
    {
        union.Int64 = l;
        type = NodeType.Int64;
    }
    public BymlNode(ulong ul)
    {
        union.UInt64 = ul;
        type = NodeType.UInt64;
    }
    public BymlNode(double d)
    {
        union.Double = d;
        type = NodeType.Double;
    }

    public static bool operator ==(BymlNode a, BymlNode b) => a.Equals(b);
    public static bool operator !=(BymlNode a, BymlNode b) => !a.Equals(b);

    public BymlNode ShallowCopy() => (BymlNode)MemberwiseClone();

    private bool IsContainerType()
    {
        return type switch {
            NodeType.Array or NodeType.Hash or NodeType.StringArray or NodeType.Binary => true,
            _ => false,
        };
    }

    public override bool Equals(object? other) => Equals(other as BymlNode);
    public virtual bool Equals(BymlNode? other)
    {
        if (this is null || other is null || type != other.type) {
            return false;
        }

        if (IsContainerType()) {
            switch (type) {
                case NodeType.Array:
                    if (Array.Count != other.Array.Count) {
                        return false;
                    }

                    for (int i = 0; i < Array.Count; i++) {
                        if (Array[i] != other.Array[i]) {
                            return false;
                        }
                    }
                    return true;
                case NodeType.Hash:
                    if (Hash.Count != other.Hash.Count) {
                        return false;
                    }

                    foreach ((string key, BymlNode child) in Hash) {
                        if (!other.Hash.TryGetValue(key, out BymlNode child2) ||
                            child != child2) {
                            return false;
                        }
                    }
                    return true;
                case NodeType.StringArray:
                    return StringArray.SequenceEqual(other.StringArray);
                case NodeType.Binary:
                    return Binary.SequenceEqual(other.Binary);
            }
        }
        return type switch {
            NodeType.String => String.Equals(other.String, StringComparison.Ordinal),
            _ => UInt64 == other.UInt64, // this works because these 8 bytes are shared by all remaining values
        };
    }

    public override int GetHashCode()
    {
        unsafe {
            return type switch {
                NodeType.Array => Array.Aggregate(0, (acc, y) => acc + y.GetHashCode()),
                NodeType.Hash => Hash.Aggregate(0, (acc, p) => acc + p.Key.GetHashCode() + p.Value.GetHashCode()),
                NodeType.StringArray => StringArray.GetHashCode(),
                NodeType.String => String.GetHashCode(),
                NodeType.Binary => Binary.GetHashCode(),
                _ => UInt64.GetHashCode(),
            };
        }
    }

    /// <summary>
    /// </summary>
    /// <returns>
    /// Returns the node value converted to a human-friendly string
    /// </returns>
    public override string ToString()
    {
        return type switch {
            NodeType.Binary => string.Join("", Binary.Select(x => x.ToString("x2"))),
            NodeType.Bool => Bool.ToString(),
            NodeType.Double => Double.ToString(),
            NodeType.Float => Float.ToString(),
            NodeType.Int => Int.ToString(),
            NodeType.Int64 => Int64.ToString(),
            NodeType.String => String,
            NodeType.StringArray => $"{string.Join(", ", StringArray)}",
            NodeType.UInt => UInt.ToString("x8"),
            NodeType.UInt64 => UInt64.ToString("x8"),
            _ => SerializeNode(),
        };
    }

    /// <summary>
    /// Serialize the current node to YAML
    /// </summary>
    /// <returns></returns>
    public string SerializeNode()
    {
        return YamlConverter.ToYaml(this);
    }
}
