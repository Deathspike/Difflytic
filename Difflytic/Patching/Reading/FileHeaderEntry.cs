using System.Runtime.InteropServices;

namespace Difflytic.Patching.Reading
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FileHeaderEntry
    {
        public bool InData;
        public long Length;
        public long LocalPosition;
        public long OtherPosition;
    }
}