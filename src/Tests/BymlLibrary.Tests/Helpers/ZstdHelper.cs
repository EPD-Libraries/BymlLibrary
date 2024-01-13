using Revrs;
using SarcLibrary;
using ZstdSharp;

namespace BymlLibrary.Tests.Helpers;

public class ZstdHelper
{
    private static readonly Lazy<ZstdHelper> _shared = new(() => new(TotkConfig.Shared.ZsDicPath));
    public static ZstdHelper Shared => _shared.Value;

    private readonly Decompressor _defaultDecompressor = new();
    private readonly Dictionary<string, Decompressor> _decompressors = [];

    public ZstdHelper(string zsDicFile)
    {
        if (File.Exists(zsDicFile)) {
            Span<byte> data = _defaultDecompressor.Unwrap(File.ReadAllBytes(zsDicFile));
            RevrsReader reader = new(data);
            ImmutableSarc sarc = new(ref reader);

            foreach ((var file, var fileData) in sarc) {
                Decompressor decompressor = new();
                decompressor.LoadDictionary(fileData);
                _decompressors[file[..file.LastIndexOf('.')]] = decompressor;
            }
        }
    }

    public Span<byte> Decompress(string file, Span<byte> src)
    {
        if (!file.EndsWith(".zs")) {
            return src;
        }

        foreach ((var key, var decompressor) in _decompressors) {
            if (file.EndsWith($"{key}.zs")) {
                return decompressor.Unwrap(src);
            }
        }

        return _decompressors["zs"].Unwrap(src);
    }
}