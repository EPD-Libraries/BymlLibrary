namespace Nintendo.Byml.Parser;

internal static class XmlTypeConverter
{
    public delegate dynamic ConvertMethod(string inString);

    public static readonly Dictionary<Type, ConvertMethod> StringToNodeTable = new()
    {
            { typeof(string) , (s) => s },
            { typeof(int) , (s) => (int.Parse(s)) },
            { typeof(uint) , (s) =>(uint.Parse(s)) },
            { typeof(long) , (s) => (long.Parse(s)) },
            { typeof(ulong) , (s) => (ulong.Parse(s)) },
            { typeof(double) , (s) =>(double.Parse(s)) },
            { typeof(float) , (s) => (float.Parse(s)) },
            { typeof(bool) , (s) => (bool.Parse(s)) },
        };

    public static dynamic ConvertValue(Type t, string value) => StringToNodeTable[t](value);
}
