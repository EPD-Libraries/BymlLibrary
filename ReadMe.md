# Sarc Library

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