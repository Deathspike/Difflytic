using System.Runtime.InteropServices;

namespace Difflytic.Diffing.Matching
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct HashTableEntry
    {
        public uint Hash;
        public long Position;
    }
}