using BymlLibrary.Structures;
using BymlLibrary.Writers;

namespace BymlLibrary.Nodes.Containers;

internal class BymlStringTable
{
    internal static int Write(BymlWriterContext writer, in List<string> strings)
    {
        int tableOffset = (int)writer.Writer.Position;

        writer.WriteContainerHeader(BymlNodeType.StringTable, strings.Count);

        int previousStringOffset = (strings.Count + 1) * sizeof(uint) + BymlContainer.SIZE;
        writer.Writer.Write(previousStringOffset);
        foreach (var str in strings) {
            writer.Writer.Write(previousStringOffset += str.Length + 1);
        }

        foreach (var str in strings) {
            writer.Writer.WriteStringUtf8(str);
            writer.Writer.Write<byte>(0);
        }

        writer.Writer.Align(4);
        return tableOffset;
    }
}
