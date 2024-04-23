#if RELEASE
using BenchmarkDotNet.Running;
using BymlLibrary.Runner.Benchmarks;

BenchmarkRunner.Run<BymlBenchmarks>();
return;
#else

using BymlLibrary;
using Revrs;

byte[] buffer = File.ReadAllBytes(args[0]);

RevrsReader reader = new(buffer);
ImmutableByml byml = new(ref reader);

string yaml = byml.ToYaml();
File.WriteAllText(args[2], yaml);

Byml fromYaml = Byml.FromText(yaml);
File.WriteAllBytes(args[1], fromYaml.ToBinary(byml.Endianness));

#endif