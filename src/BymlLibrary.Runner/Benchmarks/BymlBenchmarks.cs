using BenchmarkDotNet.Attributes;
using Revrs;

namespace BymlLibrary.Runner.Benchmarks;

[MemoryDiagnoser(true)]
public class BymlBenchmarks
{
    private readonly byte[] _buffer = File.ReadAllBytes(@"D:\bin\Byml\ActorInfo-LE.byml");
    private readonly Byml _byml;
    private readonly string _yaml;

    public BymlBenchmarks()
    {
        RevrsReader reader = new(_buffer);
        ImmutableByml byml = new(ref reader);
        _byml = Byml.FromImmutable(byml);
        _yaml = byml.ToYaml();
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
        _byml.WriteBinary(ms, Endianness.Little);
    }

    [Benchmark]
    public void ToBinary()
    {
        _ = _byml.ToBinary(Endianness.Little);
    }

    [Benchmark]
    public void ToYaml()
    {
        RevrsReader reader = new(_buffer);
        ImmutableByml byml = new(ref reader);
        string _ = byml.ToYaml();
    }

    [Benchmark]
    public void FromYaml()
    {
        Byml _ = Byml.FromText(_yaml);
    }
}
