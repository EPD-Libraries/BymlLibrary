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

| Method        |      Mean |      Gen0 |      Gen1 |     Gen2 | Allocated |
| ------------- | --------: | --------: | --------: | -------: | --------: |
| Read          |  78.26 ms | 3000.0000 | 2857.1429 | 571.4286 |  37.42 MB |
| ReadImmutable |  15.97 ns |         - |         - |        - |         - |
| Write         |  41.26 ms | 1250.0000 |  583.3333 | 250.0000 |  24.44 MB |
| ToBinary      |  41.87 ms | 1307.6923 |  615.3846 | 307.6923 |  26.32 MB |
| ToYaml        |  39.87 ms | 1615.3846 |  615.3846 | 615.3846 |  40.48 MB |
| FromYaml      | 115.69 ms | 3000.0000 | 2800.0000 | 600.0000 |  38.74 MB |

> Benchmarks for `GameData/GameDataList.Product.110.byml` (TotK 1.2.1 | **12.4 MB**)

| Method        |     Mean |       Gen0 |       Gen1 |     Gen2 | Allocated |
| ------------- | -------: | ---------: | ---------: | -------: | --------: |
| ReadImmutable | 16.52 ns |          - |          - |        - |         - |
| Read          | 276.7 ms | 11500.0000 | 11000.0000 | 500.0000 | 174.09 MB |
| Write         | 215.4 ms |  4000.0000 |          - |        - | 182.31 MB |
| ToBinary      | 247.6 ms |  5000.0000 |  1000.0000 | 500.0000 | 191.16 MB |
| ToYaml        | 169.7 ms |  5333.3333 |   333.3333 | 333.3333 | 186.38 MB |
| FromYaml      | 496.1 ms | 13000.0000 | 12000.0000 |        - | 209.17 MB |

> Benchmarks for the test file (contains one of every node in a `Map`)

| Method        |     Mean |   Gen0 |   Gen1 | Allocated |
| ------------- | -------: | -----: | -----: | --------: |
| Read          | 1.888 us | 0.2365 |      - |   3.65 KB |
| ReadImmutable | 16.13 ns |      - |      - |         - |
| Write         | 5.388 us | 0.8545 | 0.0153 |   13.2 KB |
| ToBinary      | 5.440 us | 0.9003 | 0.0229 |  13.82 KB |
| ToYaml        | 2.487 us | 0.3242 |      - |      5 KB |
| FromYaml      | 6.968 us | 0.3052 |      - |   4.76 KB |

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