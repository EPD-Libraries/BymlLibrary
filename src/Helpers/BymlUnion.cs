using System.Runtime.InteropServices;

namespace Nintendo.Byml
{
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    internal unsafe struct BymlUnion
    {
        [FieldOffset(0)]
        private bool b;
        [FieldOffset(0)]
        private int i;
        [FieldOffset(0)]
        private float f;
        [FieldOffset(0)]
        private uint u;
        [FieldOffset(0)]
        private long l;
        [FieldOffset(0)]
        private ulong ul;
        [FieldOffset(0)]
        private double d;
        [FieldOffset(8)]
        private char[] str;
        [FieldOffset(8)]
        private byte[] binary;

        public string String
        {
            get { return new string(str); }
            set { str = value.ToCharArray(); }
        }
        public byte[] Binary { get => binary; set => binary = value; }
        public bool Bool { get => b; set => b = value; }
        public int Int { get => i; set => i = value; }
        public float Float { get => f; set => f = value; }
        public uint UInt { get => u; set => u = value; }
        public long Int64 { get => l; set => l = value; }
        public ulong UInt64 { get => ul; set => ul = value; }
        public double Double { get => d; set => d = value; }
    }
}
