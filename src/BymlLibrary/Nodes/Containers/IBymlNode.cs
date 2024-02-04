using BymlLibrary.Writers;
using System.Runtime.CompilerServices;

namespace BymlLibrary.Nodes.Containers;

internal interface IBymlNode
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetValueHash();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int Collect(in BymlWriter writer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Write(BymlWriter context, Action<Byml> write);
}
