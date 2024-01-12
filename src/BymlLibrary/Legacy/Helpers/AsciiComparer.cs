namespace BymlLibrary.Legacy;

internal class AsciiComparer : IComparer<string>
{
    public int Compare(string? x, string? y)
    {
        if (x is null || y is null) {
            throw new NotImplementedException("""
                Null comparison is not implemented
                """);
        }

        int shorter_size = x.Length < y.Length ? x.Length : y.Length;
        for (int i = 0; i < shorter_size; i++) {
            if (x[i] != y[i]) {
                return (byte)x[i] - (byte)y[i];
            }
        }
        if (x.Length == y.Length) {
            return 0;
        }
        return x.Length - y.Length;
    }
}
