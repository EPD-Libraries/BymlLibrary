using System.Reflection;
using System.Text;

namespace Nintendo.Byml;

internal class Resource
{
    internal byte[] Data { get; set; } = Array.Empty<byte>();
    internal Resource(string resourceName)
    {
        Assembly assembly = Assembly.GetCallingAssembly();
        Stream resStream = assembly.GetManifestResourceStream("BymlLibrary." + resourceName);

        if (resStream == null)
            return;

        using BinaryReader reader = new(resStream);
        Data = reader.ReadBytes((int)resStream.Length);
    }

    /// <summary>
    /// Returns a UTF8 encoded string of the resource.
    /// </summary>
    /// <param name="res"></param>
    /// <returns></returns>
    public override string ToString() => Encoding.UTF8.GetString(Data);
}
