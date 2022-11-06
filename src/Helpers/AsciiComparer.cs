using System.Collections.Generic;

namespace Nintendo.Byml
{
    internal class AsciiComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            int shorter_size = x.Length < y.Length ? x.Length : y.Length;
            for (int i = 0; i < shorter_size; i++)
            {
                if (x[i] != y[i])
                {
                    return (byte)x[i] - (byte)y[i];
                }
            }
            if (x.Length == y.Length)
            {
                return 0;
            }
            return x.Length - y.Length;
        }
    }
}
