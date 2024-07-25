using System.Runtime.InteropServices;

namespace Difflytic.Diffing.Matching
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct HashTableEntry
    {
        [FieldOffset(0)] public uint Hash;
        [FieldOffset(4)] public long Position;
    }
}