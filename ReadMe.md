<div align="center">
  <img src="https://github.com/EPD-Libraries/BymlLibrary/blob/master/icon.png" width="100vh">
  <h1>- &nbsp; BYML Library &nbsp; -</h1>
</div>

Modern **B**inary **Yml** IO library written in managed C#

Supports versions **2-7**.

> [!NOTE]
> Some v7 nodes may not be supported, but everything used in TotK is.

- [Usage](#usage)
  - [Reading a Byml File](#reading-a-byml-file)
  - [Reading a Byml for Read-Only use (Much Faster)](#reading-a-byml-for-read-only-use-much-faster)
  - [Writing a Byml File](#writing-a-byml-file)
- [Benchmarks](#benchmarks)
  - [Install](#install)
    - [NuGet](#nuget)
    - [Build From Source](#build-from-source)

## Usage

### Reading a Byml File

```cs
using BymlLibrary;
using Revrs;

byte[] data = File.ReadAllBytes("path/to/file.byml");
Byml byml = Byml.FromBinary(data);
```

### Reading a Byml for Read-Only use (Much Faster)

```cs
using BymlLibrary;
using Revrs;

byte[] data = File.ReadAllBytes("path/to/file.byml");
RevrsReader reader = new(data);
ImmutableByml byml = new(ref reader);
```

### Writing a Byml File

```cs
/* ... */

// Avoid writing directly to
// a file stream. Seeking is
// much slower and used extensively
// during serialization.
using MemoryStream ms = new();
byml.WriteBinary(ms, Endianness.Little);

// Write to a byte[]
byte[] data = byml.ToBinary(Endianness.Little);
```

## Benchmarks

> Benchmarks for `Actors/ActorInfo.product.byml` (BotW for Switch | **1.9 MB**)

| Method        |      Mean |    Error |   StdDev |       Gen0 |       Gen1 |      Gen2 | Allocated |
| ------------- | --------: | -------: | -------: | ---------: | ---------: | --------: | --------: |
| Read          |  66.97 ms | 1.299 ms | 1.984 ms |  3250.0000 |  3125.0000 |  625.0000 |  40.04 MB |
| ReadImmutable |  15.97 ns | 0.245 ns | 0.217 ns |          - |          - |         - |         - |
| Write         |  35.86 ms | 0.712 ms | 1.266 ms |   666.6667 |   400.0000 |  266.6667 |  16.34 MB |
| ToBinary      |  36.02 ms | 0.714 ms | 1.376 ms |   714.2857 |   500.0000 |  285.7143 |  18.24 MB |
| ToYaml        |  35.87 ms | 0.713 ms | 0.902 ms |  1785.7143 |  1142.8571 |  214.2857 |  34.07 MB |
| FromYaml      | 384.21 ms | 7.684 ms | 9.436 ms | 14000.0000 | 13000.0000 | 1000.0000 | 199.68 MB |

> Benchmarks for `GameData/GameDataList.Product.110.byml` (TotK 1.2.1 | **12.4 MB**)

| Method        |       Mean |    Error |   StdDev |       Gen0 |       Gen1 |      Gen2 |  Allocated |
| ------------- | ---------: | -------: | -------: | ---------: | ---------: | --------: | ---------: |
| Read          |   263.8 ms |  4.49 ms |  6.58 ms | 12500.0000 | 12000.0000 |  500.0000 |  188.49 MB |
| ReadImmutable |   16.52 ns | 0.282 ns | 0.231 ns |          - |          - |         - |          - |
| Write         |   275.8 ms |  5.48 ms | 14.43 ms |  2000.0000 |  1000.0000 |         - |  153.85 MB |
| ToBinary      |   275.0 ms |  5.49 ms | 14.76 ms |  2000.0000 |  1000.0000 |         - |  164.53 MB |
| ToYaml        |   133.6 ms |  2.54 ms |  2.61 ms |  5500.0000 |  4500.0000 |  250.0000 |  117.81 MB |
| FromYaml      | 1,960.0 ms | 36.65 ms | 34.28 ms | 71000.0000 | 43000.0000 | 2000.0000 | 1043.85 MB |

> Benchmarks for the test file (contains one of every node in a `Map`)

| Method        |      Mean |     Error |    StdDev |   Gen0 |   Gen1 | Allocated |
| ------------- | --------: | --------: | --------: | -----: | -----: | --------: |
| Read          |  1.587 μs | 0.0154 μs | 0.0144 μs | 0.2384 | 0.0019 |   3.68 KB |
| ReadImmutable |  16.13 ns |  0.074 ns |  0.066 ns |      - |      - |         - |
| Write         |  4.486 μs | 0.0872 μs | 0.1004 μs | 0.6332 | 0.0076 |   9.73 KB |
| ToBinary      |  4.370 μs | 0.0530 μs | 0.0442 μs | 0.6714 | 0.0076 |  10.35 KB |
| ToYaml        |  2.888 μs | 0.0453 μs | 0.0485 μs | 0.3090 |      - |   4.78 KB |
| FromYaml      | 23.004 μs | 0.2572 μs | 0.2280 μs | 2.0447 | 0.1221 |  31.73 KB |

> [!NOTE]
> `ns` (nanoseconds) is not `μs` (microseconds)

### Install

[![NuGet](https://img.shields.io/nuget/v/BymlLibrary.svg?style=for-the-badge&labelColor=2a2c33)](https://www.nuget.org/packages/BymlLibrary) [![NuGet](https://img.shields.io/nuget/dt/BymlLibrary.svg?style=for-the-badge&labelColor=2a2c33&color=32a852)](https://www.nuget.org/packages/BymlLibrary)

#### NuGet
```powershell
Install-Package BymlLibrary
```

#### Build From Source
```batch
git clone https://github.com/EPD-Libraries/BymlLibrary.git
dotnet build BymlLibrary
```

Special thanks to **[Léo Lam](https://github.com/leoetlino)** for his extensive research on EPD file formats.