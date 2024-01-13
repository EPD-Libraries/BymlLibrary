# Byml Library

Modern **B**inary **Yml** IO library written in managed C#

## Usage

### Reading a Byml File

```cs
byte[] data = File.ReadAllBytes("path/to/file.byml");
Byml byml = Byml.FromBinary(data);
```

### Writing a Byml File

```cs
/* ... */

using MemoryStream ms = new();
byml.Write(ms);
```

## Benchmarks

> Benchmarks for `Actors/ActorInfo.product.byml` (BotW for Switch)

| Method        |        Mean |    Error |   StdDev |       Gen0 |       Gen1 |      Gen2 | Allocated |
| ------------- | ----------: | -------: | -------: | ---------: | ---------: | --------: | --------: |
| Read          |  66.53 `ms` | 1.312 ms | 1.923 ms |  3250.0000 |  3125.0000 |  625.0000 |  40.04 MB |
| ReadImmutable |  15.97 `ns` | 0.245 ns | 0.217 ns |          - |          - |         - |         - |
| Write         |  26.62 `ms` | 0.530 ms | 0.886 ms |   531.2500 |   375.0000 |  250.0000 |  12.79 MB |
| ToBinary      |  27.09 `ms` | 0.540 ms | 1.384 ms |   593.7500 |   437.5000 |  312.5000 |  14.66 MB |
| ToYaml        |  35.63 `ms` | 0.574 ms | 0.537 ms |  1785.7143 |  1500.0000 |  214.2857 |  33.94 MB |
| FromYaml      | 383.30 `ms` | 6.531 ms | 5.790 ms | 14000.0000 | 13000.0000 | 1000.0000 | 198.88 MB |

> Benchmarks for the test file (contains one of every node in a `Map`)

| Method        |        Mean |     Error |    StdDev |   Gen0 |   Gen1 | Allocated |
| ------------- | ----------: | --------: | --------: | -----: | -----: | --------: |
| Read          |  1.587 `us` | 0.0154 us | 0.0144 us | 0.2384 | 0.0019 |   3.68 KB |
| ReadImmutable |  16.13 `ns` |  0.074 ns |  0.066 ns |      - |      - |         - |
| Write         |  4.486 `us` | 0.0872 us | 0.1004 us | 0.6332 | 0.0076 |   9.73 KB |
| ToBinary      |  4.370 `us` | 0.0530 us | 0.0442 us | 0.6714 | 0.0076 |  10.35 KB |
| ToYaml        |  2.888 `us` | 0.0453 us | 0.0485 us | 0.3090 |      - |   4.78 KB |
| FromYaml      | 23.004 `us` | 0.2572 us | 0.2280 us | 2.0447 | 0.1221 |  31.73 KB |

### Install

[![NuGet](https://img.shields.io/nuget/v/BymlLibrary.svg)](https://www.nuget.org/packages/BymlLibrary) [![NuGet](https://img.shields.io/nuget/dt/BymlLibrary.svg)](https://www.nuget.org/packages/BymlLibrary)

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