#if RELEASE
using BenchmarkDotNet.Running;
using BymlLibrary.Runner.Benchmarks;

BenchmarkRunner.Run<BymlBenchmarks>();
return;
#else
using BymlLibrary;
using BymlLibrary.Yaml;
using Revrs;

byte[] buffer = File.ReadAllBytes(args[0]);
RevrsReader reader = new(buffer);
ImmutableByml immutableByml = new(ref reader);

YamlEmitter emitter = new();
emitter.Emit(immutableByml);

File.WriteAllText("D:\\bin\\Byml-v7\\test.yml", emitter.Builder.ToString());

using FileStream fs = File.Create(args[1]);
Byml byml = Byml.FromBinary(buffer);
byml.WriteBinary(fs, byml.Endianness);

// Console.WriteLine(byml.GetMap()["PtclBin"].GetBinaryAligned().Alignment);
// Console.WriteLine(
//     byml
//         .Get<IDictionary<string, Byml>>()["Actors"]
//         .Get<IList<Byml>>()[3]
//         .Get<IDictionary<string, Byml>>()["name"]
//         .Get<string>()
// );
#endif