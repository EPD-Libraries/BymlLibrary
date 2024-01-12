using BenchmarkDotNet.Attributes;
using BymlLibrary.Legacy;
using Revrs;

namespace BymlLibrary.Runner.Benchmarks;

[MemoryDiagnoser(true)]
public class BymlBenchmarks
{
    private readonly byte[] _buffer = File.ReadAllBytes(@"D:\bin\Byml\ActorInfo-LE.byml");
    private readonly Byml _byml;
    private readonly BymlFile _legacyByml;

    public BymlBenchmarks()
    {
        _byml = Byml.FromBinary(_buffer);
        _legacyByml = BymlFile.FromBinary(_buffer);
    }

    [Benchmark]
    public void Read()
    {
        Byml _ = Byml.FromBinary(_buffer);
    }

    [Benchmark]
    public void ReadImmutable()
    {
        RevrsReader reader = new(_buffer);
        ImmutableByml _ = new(ref reader);
    }

    [Benchmark]
    public void Write()
    {
        using MemoryStream ms = new();
        _byml.WriteBinary(ms, _byml.Endianness);
    }

    [Benchmark]
    public void ToBinary()
    {
        _ = _byml.ToBinary(_byml.Endianness);
    }

    [Benchmark]
    public void ToYaml()
    {
        RevrsReader reader = new(_buffer);
        ImmutableByml byml = new(ref reader);
        string _ = byml.ToYaml();
    }

    [Benchmark]
    public void LegacyRead()
    {
        BymlFile _ = BymlFile.FromBinary(_buffer);
    }

    [Benchmark]
    public void LegacyWrite()
    {
        byte[] _ = _legacyByml.ToBinary();
    }

    [Benchmark]
    public void LegacyToYaml()
    {
        string _ = _legacyByml.ToYaml();
    }
}
