using BymlLibrary.Tests.Helpers;
using Revrs;
using SarcLibrary;
using Yaz0Library;

namespace BymlLibrary.Tests;

public class GameTests
{
    private const string ZS_EXT = ".zs";

    // Yes, I hardcoded my game paths
    public const string TOTK_PATH = "D:\\Games\\Emulation\\RomFS\\Totk\\Latest";
    public const string BOTW_PATH = "D:\\Games\\Emulation\\RomFS\\Botw\\1.5.0";
    public const string BOTW_PATH_NX = "D:\\Games\\Emulation\\RomFS\\Botw\\1.6.0";

    private static readonly HashSet<string> _totkSarcExtensions = [
        ".sarc",
        ".pack",
        ".bars",
        ".ta",
        ".blarc",
        ".bkres",
        ".bfarc",
        ".genvb"
    ];

    private static readonly HashSet<string> _totkBymlExtensions = [
        ".bgyml",
        ".byaml",
        ".byml"
    ];

    private static readonly HashSet<string> _botwSarcExtensions = [
        ".sarc",
        ".pack",
        ".bactorpack",
        ".bmodelsh",
        ".beventpack",
        ".stera",
        ".stats",
        ".ssarc",
        ".spack",
        ".sbactorpack",
        ".sbmodelsh",
        ".sbeventpack",
        ".sstera",
        ".sstats"
    ];

    private static readonly HashSet<string> _botwBymlExtensions = [
        ".bgdata",
        ".sbgdata",
        ".bquestpack",
        ".sbquestpack",
        ".byml",
        ".sbyml",
        ".mubin",
        ".smubin",
        ".baischedule",
        ".sbaischedule",
        ".baniminfo",
        ".sbaniminfo",
        ".bgsvdata",
        ".sbgsvdata"
    ];

    [Fact]
    public void ReadTotkFiles()
    {
        foreach (var file in Directory.GetFiles(TOTK_PATH, "*.*", SearchOption.AllDirectories)) {
            if (IsTotkByml(file, null, out Span<byte> byml)) {
                Byml _ = Byml.FromBinary(byml);
                continue;
            }

            if (IsTotkSarc(file, null, out Span<byte> sarcData) && sarcData[..4].SequenceEqual("SARC"u8)) {
                RevrsReader reader = new(sarcData);
                ImmutableSarc sarc = new(ref reader);
                ReadTotkSarc(sarc);
            }
        }
    }

    [Fact]
    public void ReadBotwWiiuFiles()
    {
        ReadBotwFiles(BOTW_PATH);
    }

    [Fact]
    public void ReadBotwSwitchFiles()
    {
        ReadBotwFiles(BOTW_PATH_NX);
    }

    private static void ReadTotkSarc(in ImmutableSarc sarc)
    {
        foreach ((var file, var data) in sarc) {
            if (IsTotkByml(file, data, out Span<byte> byml)) {
                Byml _ = Byml.FromBinary(byml);
                continue;
            }

            if (IsTotkSarc(file, data, out Span<byte> sarcData) && sarcData[..4].SequenceEqual("SARC"u8)) {
                RevrsReader reader = new(sarcData);
                ImmutableSarc subSarc = new(ref reader);
                ReadTotkSarc(subSarc);
            }
        }
    }

    private static void ReadBotwFiles(string path)
    {
        foreach (var file in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)) {
            if (IsBotwByml(file, null, out Span<byte> byml)) {
                Byml _ = Byml.FromBinary(byml);
                continue;
            }

            if (IsBotwSarc(file, null, out Span<byte> sarcData)) {
                RevrsReader reader = new(sarcData);
                ImmutableSarc sarc = new(ref reader);
                ReadBotwSarc(sarc);
            }
        }
    }

    private static void ReadBotwSarc(in ImmutableSarc sarc)
    {
        foreach ((var file, var data) in sarc) {
            if (IsBotwByml(file, data, out Span<byte> byml)) {
                Byml _ = Byml.FromBinary(byml);
                continue;
            }

            if (IsBotwSarc(file, data, out Span<byte> sarcData)) {
                RevrsReader reader = new(sarcData);
                ImmutableSarc subSarc = new(ref reader);
                ReadBotwSarc(subSarc);
            }
        }
    }

    private static bool IsTotkByml(string path, Span<byte> src, out Span<byte> data)
    {
        return IsAnyTargets(path, _totkBymlExtensions, src, out data);
    }

    private static bool IsTotkSarc(string path, Span<byte> src, out Span<byte> data)
    {
        return IsAnyTargets(path, _totkSarcExtensions, src, out data);
    }

    private static bool IsBotwByml(string path, Span<byte> src, out Span<byte> data)
    {
        bool isByml = IsAnyTargets(path, _botwBymlExtensions, src, out data);
        data = TryDecompressYaz0(data);
        return isByml;
    }

    private static bool IsBotwSarc(string path, Span<byte> src, out Span<byte> data)
    {
        bool isSarc = IsAnyTargets(path, _botwSarcExtensions, src, out data);
        data = TryDecompressYaz0(data);
        return isSarc;
    }

    private static bool IsAnyTargets(string path, HashSet<string> targets, Span<byte> src, out Span<byte> data)
    {
        bool isZs = IsZs(path, out string ext);

        if (targets.Contains(ext)) {
            if (src.IsEmpty) {
                src = File.ReadAllBytes(path);
            }

            data = isZs ? ZstdHelper.Shared.Decompress(path, src) : src;
            return true;
        }

        data = [];
        return false;
    }

    private static bool IsZs(string path, out string ext)
    {
        ext = Path.GetExtension(path);
        bool isZs = ext == ZS_EXT;
        if (isZs) {
            ext = Path.GetExtension(path[..^3]);
        }

        return isZs;
    }

    private static Span<byte> TryDecompressYaz0(Span<byte> src)
    {
        if (src.Length >= 4 && src[..4].SequenceEqual("Yaz0"u8)) {
            return Yaz0.Decompress(src);
        }

        return src;
    }
}
