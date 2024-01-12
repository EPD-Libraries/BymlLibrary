#if RELEASE
using BenchmarkDotNet.Running;
using BymlLibrary.Runner.Benchmarks;

BenchmarkRunner.Run<BymlBenchmarks>();
return;
#else

using BymlLibrary;
using Revrs;

byte[] buffer = File.ReadAllBytes(args[1]);
RevrsReader reader = new(buffer);
ImmutableByml immutableByml = new(ref reader);

string yaml = immutableByml.ToYaml();
File.WriteAllText("D:\\bin\\Byml-v7\\test.yml", yaml);

Byml byml = Byml.FromText(yaml);
Console.WriteLine(byml.GetMap()["Actors"].GetArray()[0].GetMap()["name"].GetString());

// Byml byml = Byml.FromBinary(buffer);
// byml.WriteBinary(args[1], Endianness.Big);

// Console.WriteLine(byml.GetMap()["PtclBin"].GetBinaryAligned().Alignment);
// Console.WriteLine(
//     byml
//         .Get<IDictionary<string, Byml>>()["Actors"]
//         .Get<IList<Byml>>()[3]
//         .Get<IDictionary<string, Byml>>()["name"]
//         .Get<string>()
// );
#endif