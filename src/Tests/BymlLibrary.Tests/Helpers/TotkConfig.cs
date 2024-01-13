using System.Text.Json;
using System.Text.Json.Serialization;

namespace BymlLibrary.Tests.Helpers;

public class TotkConfig
{
    private static readonly string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Totk", "config.json");

    private static readonly Lazy<TotkConfig> _shared = new(Load);
    public static TotkConfig Shared => _shared.Value;

    public required string GamePath { get; set; }

    [JsonIgnore]
    public string ZsDicPath => Path.Combine(GamePath, "Pack", "ZsDic.pack.zs");

    public static TotkConfig Load()
    {
        if (!File.Exists(_path)) {
            return Create();
        }

        using FileStream fs = File.OpenRead(_path);
        return JsonSerializer.Deserialize(fs, TotkConfigSerializerContext.Default.TotkConfig) ?? Create();
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        using FileStream fs = File.Create(_path);
        JsonSerializer.Serialize(fs, this, TotkConfigSerializerContext.Default.TotkConfig);
    }

    private static TotkConfig Create()
    {
        TotkConfig config = new() {
            GamePath = string.Empty
        };

        config.Save();
        return config;
    }
}

[JsonSerializable(typeof(TotkConfig))]
public partial class TotkConfigSerializerContext : JsonSerializerContext
{

}