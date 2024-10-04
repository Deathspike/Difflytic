using System.Runtime.InteropServices;

namespace Difflytic.Patching.Reading.IO
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DiffStreamHeaderEntry
    {
        public bool InData;
        public long Length;
        public long LocalPosition;
        public long OtherPosition;
    }
}