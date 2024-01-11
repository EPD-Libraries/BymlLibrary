using BymlLibrary.Structures;
using Revrs;

namespace BymlLibrary.Nodes.Containers;

public class BymlStringTable
{
    public static int Write(in RevrsWriter writer, in List<string> strings)
    {
        int tableOffset = (int)writer.Position;

        BymlContainer header = new(BymlNodeType.StringTable, strings.Count);
        writer.Write<BymlContainer, BymlContainer.Reverser>(header);
        writer.Write(tableOffset);

        foreach (var str in strings) {
            writer.Write(tableOffset + str.Length + 1);
        }

        writer.Align(4);

        foreach (var str in strings) {
            writer.WriteStringUtf8(str);
        }

        return tableOffset;
    }
}
