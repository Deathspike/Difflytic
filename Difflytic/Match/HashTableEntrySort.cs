using System.Collections.Generic;

namespace Difflytic.Match
{
    public sealed class HashTableEntrySort : IComparer<HashTableEntry>
    {
        public static HashTableEntrySort Instance = new();

        #region Implementation of IComparer<HashTableEntry>

        public int Compare(HashTableEntry x, HashTableEntry y)
        {
            var hashCompare = x.Hash.CompareTo(y.Hash);
            if (hashCompare != 0) return hashCompare;
            return x.Position.CompareTo(y.Position);
        }

        #endregion
    }
}