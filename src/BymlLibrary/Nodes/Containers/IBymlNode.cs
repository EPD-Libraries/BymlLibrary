using BymlLibrary.Writers;
using Revrs;
using System.Runtime.CompilerServices;

namespace BymlLibrary.Nodes.Containers;

internal interface IBymlNode
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int Collect(in BymlWriter writer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Write(RevrsWriter writer);
}
