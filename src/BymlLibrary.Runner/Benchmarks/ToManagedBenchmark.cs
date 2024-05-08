using BenchmarkDotNet.Attributes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace BymlLibrary.Runner.Benchmarks;

[MemoryDiagnoser(true)]
public unsafe class ToManagedBenchmark
{
    private readonly byte[] _managed = "Some Arbitrary String Value"u8.ToArray();

    [Benchmark]
    public string ToManagedUtf8StringMarshal()
    {
        Span<byte> utf8 = _managed;
        fixed (byte* ptr = utf8) {
            return Utf8StringMarshaller.ConvertToManaged(ptr)
                ?? string.Empty;
        }
    }

    [Benchmark]
    public string ToManagedNoStrlenAsPtr()
    {
        Span<byte> utf8 = _managed;
        return Marshal.PtrToStringUTF8((IntPtr)Unsafe.AsPointer(ref utf8[0]), utf8.Length);
    }

    [Benchmark]
    public string? ToManagedNoStrlenFixed()
    {
        Span<byte> utf8 = _managed;
        fixed (byte* ptr = utf8) {
            return Marshal.PtrToStringUTF8((IntPtr)ptr, utf8.Length);
        }
    }
}
