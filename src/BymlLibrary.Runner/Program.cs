using BenchmarkDotNet.Running;
using BymlLibrary;
using BymlLibrary.Runner.Benchmarks;
using BymlLibrary.Yaml;
using Revrs;

BenchmarkRunner.Run<BymlBenchmarks>();
return;

byte[] buffer = File.ReadAllBytes(args[0]);
RevrsReader reader = new(buffer);
ImmutableByml immutableByml = new(ref reader);

YamlEmitter emitter = new();
emitter.Emit(ref immutableByml);

File.WriteAllText("D:\\bin\\Byml-v7\\test.yml", emitter.Builder.ToString());

Byml byml = Byml.FromBinary(buffer);
// Console.WriteLine(
//     byml
//         .Get<IDictionary<string, Byml>>()["Actors"]
//         .Get<IList<Byml>>()[3]
//         .Get<IDictionary<string, Byml>>()["name"]
//         .Get<string>()
// );
