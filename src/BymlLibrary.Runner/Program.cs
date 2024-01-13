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
ImmutableByml immutableByml = new(ref reader);

string yaml = immutableByml.ToYaml();
File.WriteAllText("./Output.yml", yaml);

Byml byml = Byml.FromText(yaml);
byml.WriteBinary(args[1], immutableByml.Endianness);

#endif