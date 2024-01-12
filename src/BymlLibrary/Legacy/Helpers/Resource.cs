using System.Reflection;
using System.Text;

namespace BymlLibrary.Legacy;

internal class Resource
{
    internal byte[] Data { get; set; } = [];
    internal Resource(string resourceName)
    {
        Assembly assembly = Assembly.GetCallingAssembly();
        Stream? resStream = assembly.GetManifestResourceStream("BymlLibrary." + resourceName);

        if (resStream is null) {
            return;
        }

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
