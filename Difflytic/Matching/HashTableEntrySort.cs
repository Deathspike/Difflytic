using System.Collections.Generic;

namespace Difflytic.Matching
{
    public sealed class HashTableEntrySort : IComparer<HashTableEntry>
    {
        public static readonly HashTableEntrySort Instance = new();

        #region Constructors

        private HashTableEntrySort()
        {
        }

        #endregion

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