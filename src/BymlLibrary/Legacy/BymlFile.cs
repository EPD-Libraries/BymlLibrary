using BymlLibrary.Legacy.IO;
using BymlLibrary.Legacy.Parser;
using Syroot.BinaryData.Core;
using System.Text;

namespace BymlLibrary.Legacy;

[Obsolete("BymlFile is obsolete, use Byml")]
public class BymlFile
{
    public Endian Endianness { get; set; } = Endian.Little;
    public BymlNode RootNode { get; set; } = new();
    public bool SupportPaths { get; set; } = false;
    public ushort Version { get; set; } = 2;

    public static BymlFile FromBinary(byte[] bytes)
    {
        using MemoryStream stream = new(bytes);
        BymlReader reader = new();
        return reader.Read(stream);
    }

    public static BymlFile FromBinary(Stream stream)
    {
        BymlReader reader = new();
        return reader.Read(stream);
    }

    public static BymlFile FromYaml(string text)
    {
        return YamlConverter.FromYaml(text);
    }

    public static BymlFile FromXml(string text)
    {
        return XmlConverter.FromXml(text);
    }

    internal BymlFile() { }

    public byte[] ToBinary()
    {
        return BymlWriter.Write(this, Encoding.UTF8);
    }

    public string ToXml()
    {
        return XmlConverter.ToXml(this);
    }

    public string ToYaml()
    {
        return YamlConverter.ToYaml(this);
    }
}
