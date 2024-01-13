using BymlLibrary.Extensions;
using System.Buffers;
using System.Text;

namespace BymlLibrary.Yaml;

internal class YamlEmitter
{
    private const string INDENT = "  ";
    private const char NEWLINE_CHAR = '\n';
    private const byte NEWLINE_CHAR_UTF8 = (byte)'\n';

    public static readonly SearchValues<byte> SpecialChars
        = SearchValues.Create("![]{}:-"u8);

    public StringBuilder Builder { get; } = new();
    public int Level { get; set; } = 0;
    public bool IsIndented { get; set; } = true;
    public bool IsInline { get; set; } = true;

    public void Emit(in ImmutableByml root)
    {
        EmitNode(root, root);
    }

    public void EmitNode(in ImmutableByml byml, in ImmutableByml root)
    {
        if (byml.Type == BymlNodeType.HashMap32) {
            byml.GetHashMap32().EmitYaml(this, root);
        }
        else if (byml.Type == BymlNodeType.HashMap64) {
            byml.GetHashMap64().EmitYaml(this, root);
        }
        else if (byml.Type == BymlNodeType.String) {
            EmitString(root.StringTable[byml.GetStringIndex()]);
        }
        else if (byml.Type == BymlNodeType.Binary) {
            Builder.Append("!!binary ");
            Builder.Append(Convert.ToBase64String(byml.GetBinary()));
        }
        else if (byml.Type == BymlNodeType.BinaryAligned) {
            Builder.Append("!!file {Alignment: ");
            Span<byte> data = byml.GetBinaryAligned(out int alignment);
            Builder.Append(alignment);
            Builder.Append(", Data: ");
            Builder.Append(Convert.ToBase64String(data));
            Builder.Append('}');
        }
        else if (byml.Type == BymlNodeType.Array) {
            byml.GetArray().EmitYaml(this, root);
        }
        else if (byml.Type == BymlNodeType.Map) {
            byml.GetMap().EmitYaml(this, root);
        }
        else if (byml.Type == BymlNodeType.StringTable) {
            throw new NotSupportedException($"""
                YAML serialization for '{byml.Type}' nodes is not supported.
                """);
        }
        else if (byml.Type == BymlNodeType.Bool) {
            Builder.Append(byml.GetBool());
        }
        else if (byml.Type == BymlNodeType.Int) {
            Builder.Append(byml.GetInt());
        }
        else if (byml.Type == BymlNodeType.Float) {
            float value = byml.GetFloat();
            Builder.Append(
                (value % 1) == 0 ? $"{value:0.0}" : value.ToString()
            );
        }
        else if (byml.Type == BymlNodeType.UInt32) {
            Builder.Append("!u ");
            Builder.Append($"0x{byml.GetUInt32():x}");
        }
        else if (byml.Type == BymlNodeType.Int64) {
            Builder.Append("!l ");
            Builder.Append(byml.GetInt64());
        }
        else if (byml.Type == BymlNodeType.UInt64) {
            Builder.Append("!ul ");
            Builder.Append($"0x{byml.GetUInt64():x}");
        }
        else if (byml.Type == BymlNodeType.Double) {
            Builder.Append("!d ");
            Builder.Append(byml.GetDouble());
        }
        else if (byml.Type == BymlNodeType.Null) {
            Builder.Append("null");
        }
        else {
            throw new NotImplementedException($"""
                YAML serialization for '{byml.Type}' nodes is not implemented.
                """);
        }
    }

    public unsafe void EmitString(Span<byte> str)
    {
        if (str.ContainsAny(SpecialChars)) {
            Builder.Append('\'');
            Builder.Append(str.ToManaged());
            Builder.Append('\'');
            return;
        }

        if (!str.Contains(NEWLINE_CHAR_UTF8)) {
            Builder.Append(str.ToManaged());
            return;
        }

        Level++;
        int index = 0;

        while ((index = str[index..].IndexOf(NEWLINE_CHAR_UTF8)) > -1) {
            Builder.AppendLine("|-");
            IndentLine();
            Builder.Append(str[..index].ToManaged());
            Builder.Append(NEWLINE_CHAR);
        }

        Level--;
    }

    public void IndentLine(int offset = 0)
    {
        if (IsIndented) {
            return;
        }

        int count = Level + offset;
        for (int i = 0; i < count; i++) {
            Builder.Append(INDENT);
        }
    }

    public void NewLine()
    {
        Builder.Append(NEWLINE_CHAR);
    }
}
