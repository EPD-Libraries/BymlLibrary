using BymlLibrary.Nodes.Containers;
using BymlLibrary.Nodes.Containers.HashMap;
using BymlLibrary.Tests.Bogus;
using Revrs;

namespace BymlLibrary.Tests;

public class BymlOperationsTests
{
    const string YAML = """
        HashMap32: !h32 {0x00000000: 1073741823, 0x7fffffff: HashMap32_Value}
        HashMap64: !h64 {0x0000000000000000: !l 4611686018427387903, 0x7fffffffffffffff: HashMap64_Value}
        String: String Value
        Binary: !!binary QmluYXJ5IFZhbHVl
        BinaryAligned: !!file {Alignment: 16, Data: !!binary QmluYXJ5IFZhbHVlIEFsaQ==}
        Map: {MapKey_A: 3.1415927, MapKey_B: Map_Value}
        Array: [!d NaN, Array_Value]
        Bool: true
        Int: 2147483647
        Float: 3.1415927
        UInt32: !u 0xffffffff
        Int64: !l 9223372036854775807
        UInt64: !ul 0xffffffffffffffff
        Double: !d 3.141592653589793
        Null: null
        """;

    [Fact]
    public static void VerifyBinary_LE()
    {
        Byml root = BymlGenerator.CreateWithEveryType();
        byte[] data = root.ToBinary(Endianness.Little);
        Verify(Byml.FromBinary(data));
    }

    [Fact]
    public static void VerifyBinary_BE()
    {
        Byml root = BymlGenerator.CreateWithEveryType();
        byte[] data = root.ToBinary(Endianness.Big);
        Verify(Byml.FromBinary(data));
    }

    [Fact]
    public static void VerifyYaml()
    {
        Byml root = BymlGenerator.CreateWithEveryType();
        byte[] data = root.ToBinary(Endianness.Little);
        RevrsReader reader = new(data);
        ImmutableByml byml = new(ref reader);
        string yaml = byml.ToYaml();
        yaml.Should().Be(YAML.Replace("\r\n", "\n"));

        Byml fromYaml = Byml.FromText(yaml);
        Verify(fromYaml);
    }

    /// <summary>
    /// Expects BymlGenerator.CreateWithEveryType
    /// </summary>
    /// <param name="byml"></param>
    private static void Verify(Byml byml)
    {
        BymlMap root = ShouldBe<BymlMap>(byml, BymlNodeType.Map);

        BymlHashMap32 hashMap32 = ShouldBe<BymlHashMap32>(root, BymlNodeType.HashMap32);
        hashMap32[uint.MinValue / 2].Type.Should().Be(BymlNodeType.Int);
        hashMap32[uint.MinValue / 2].Get<int>().Should().Be(int.MaxValue / 2);
        hashMap32[uint.MaxValue / 2].Type.Should().Be(BymlNodeType.String);
        hashMap32[uint.MaxValue / 2].GetString().Should().Be("HashMap32_Value");

        BymlHashMap64 hashMap64 = ShouldBe<BymlHashMap64>(root, BymlNodeType.HashMap64);
        hashMap64[ulong.MinValue / 2].Type.Should().Be(BymlNodeType.Int64);
        hashMap64[ulong.MinValue / 2].Get<long>().Should().Be(long.MaxValue / 2);
        hashMap64[ulong.MaxValue / 2].Type.Should().Be(BymlNodeType.String);
        hashMap64[ulong.MaxValue / 2].GetString().Should().Be("HashMap64_Value");

        string str = ShouldBe<string>(root, BymlNodeType.String);
        str.Should().Be("String Value");

        byte[] data = ShouldBe<byte[]>(root, BymlNodeType.Binary);
        data.AsSpan().SequenceEqual("Binary Value"u8).Should().BeTrue();

        (byte[] dataAligned, int alignment) = ShouldBe<(byte[], int)>(root, BymlNodeType.BinaryAligned);
        dataAligned.AsSpan().SequenceEqual("Binary Value Ali"u8).Should().BeTrue();
        alignment.Should().Be(16);

        BymlMap map = ShouldBe<BymlMap>(root, BymlNodeType.Map);
        map["MapKey_A"].Type.Should().Be(BymlNodeType.Float);
        map["MapKey_A"].Get<float>().Should().Be(float.Pi);
        map["MapKey_B"].Type.Should().Be(BymlNodeType.String);
        map["MapKey_B"].GetString().Should().Be("Map_Value");

        BymlArray array = ShouldBe<BymlArray>(root, BymlNodeType.Array);
        array.Count.Should().Be(2);
        array[0].Type.Should().Be(BymlNodeType.Double);
        array[0].Get<double>().Should().Be(double.NaN);
        array[1].Type.Should().Be(BymlNodeType.String);
        array[1].GetString().Should().Be("Array_Value");

        bool boolean = ShouldBe<bool>(root, BymlNodeType.Bool);
        boolean.Should().BeTrue();

        int int32 = ShouldBe<int>(root, BymlNodeType.Int);
        int32.Should().Be(int.MaxValue);

        float f32 = ShouldBe<float>(root, BymlNodeType.Float);
        f32.Should().Be(float.Pi);

        uint uint32 = ShouldBe<uint>(root, BymlNodeType.UInt32);
        uint32.Should().Be(uint.MaxValue);

        long int64 = ShouldBe<long>(root, BymlNodeType.Int64);
        int64.Should().Be(long.MaxValue);

        ulong uint64 = ShouldBe<ulong>(root, BymlNodeType.UInt64);
        uint64.Should().Be(ulong.MaxValue);

        double f64 = ShouldBe<double>(root, BymlNodeType.Double);
        f64.Should().Be(double.Pi);

        root[BymlNodeType.Null.ToString()].Type.Should().Be(BymlNodeType.Null);
    }

    private static T ShouldBe<T>(Byml byml, BymlNodeType type)
    {
        byml.Type.Should().Be(type);
        return byml.Get<T>();
    }

    private static T ShouldBe<T>(BymlMap map, BymlNodeType type)
    {
        Byml byml = map[type.ToString()];
        byml.Type.Should().Be(type);
        return byml.Get<T>();
    }
}
