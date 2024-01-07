using BymlLibrary;
using BymlLibrary.Yaml;
using Revrs;

byte[] buffer = File.ReadAllBytes(args[0]);
RevrsReader reader = new(buffer);

ImmutableByml byml = new(ref reader);

YamlEmitter emitter = new();
emitter.Emit(ref byml);

File.WriteAllText("D:\\bin\\Byml-v7\\test.yml", emitter.Builder.ToString());