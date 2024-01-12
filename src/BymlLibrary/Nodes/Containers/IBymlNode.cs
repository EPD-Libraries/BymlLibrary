using BymlLibrary.Writers;
using System.Runtime.CompilerServices;

namespace BymlLibrary.Nodes.Containers;

internal interface IBymlNode
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int Collect(in BymlWriterContext writer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int Write(BymlWriterContext context);
}
