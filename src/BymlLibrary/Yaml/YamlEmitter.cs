using BymlLibrary.Extensions;
using System.Buffers;
using System.Text;

namespace BymlLibrary.Yaml;

public class YamlEmitter
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

    public void Emit(ref ImmutableByml root)
    {
        EmitNode(root, root);
    }

    public void EmitNode(in ImmutableByml byml, in ImmutableByml root)
    {
        if (byml.Type.IsHasMap()) {
            byml.GetHashMap().EmitYaml(this, root);
        }
        else if (byml.Type == BymlNodeType.RemappedHashMap) {

        }
        else if (byml.Type == BymlNodeType.String) {
            EmitString(root.StringTable[byml.GetStringIndex()]);
        }
        else if (byml.Type == BymlNodeType.Binary) {
            Builder.Append("!!binary ");
            Builder.Append(Convert.ToBase64String(byml.GetBinary()));
        }
        else if (byml.Type == BymlNodeType.BinaryAligned) {

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
        else if (byml.Type == BymlNodeType.RemappedMap) {
            // TODO: RemappedMap support
        }
        else if (byml.Type == BymlNodeType.RelocatedStringTable) {
            // TODO: RelocatedStringTable support
        }
        else if (byml.Type == BymlNodeType.MonoTypedArray) {
            // TODO: MonoTypedArray support
        }
        else if (byml.Type == BymlNodeType.Bool) {
            Builder.Append(byml.GetBool());
        }
        else if (byml.Type == BymlNodeType.Int) {
            Builder.Append(byml.GetInt());
        }
        else if (byml.Type == BymlNodeType.Float) {
            Builder.Append(byml.GetFloat());
        }
        else if (byml.Type == BymlNodeType.UInt32) {
            Builder.Append("!u ");
            Builder.Append($"0x{byml.GetUInt32():X2}");
        }
        else if (byml.Type == BymlNodeType.Int64) {
            Builder.Append("!s64 ");
            Builder.Append(byml.GetInt64());
        }
        else if (byml.Type == BymlNodeType.UInt64) {
            Builder.Append("!u64 ");
            Builder.Append($"0x{byml.GetUInt64():X2}");
        }
        else if (byml.Type == BymlNodeType.Double) {
            Builder.Append("!f64 ");
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
            AppendUtf8Bytes(str);
            Builder.Append('\'');
            return;
        }

        if (!str.Contains(NEWLINE_CHAR_UTF8)) {
            AppendUtf8Bytes(str);
            return;
        }

        Level++;
        int index = 0;

        while ((index = str[index..].IndexOf(NEWLINE_CHAR_UTF8)) > -1) {
            Builder.AppendLine("|-");
            IndentLine();
            AppendUtf8Bytes(str[..index]);
            Builder.Append(NEWLINE_CHAR);
        }

        Level--;
    }

    private unsafe void AppendUtf8Bytes(Span<byte> str)
    {
        // TODO: Check that this is actually
        // faster than allocating a string.
        for (int i = 0; i < str.Length; i++) {
            if (str[i] == 0) {
                break;
            }

            Builder.Append((char)str[i]);
        }
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
