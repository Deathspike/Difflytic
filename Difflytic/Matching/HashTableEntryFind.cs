using System.Collections.Generic;

namespace Difflytic.Matching
{
    public sealed class HashTableEntryFind : IComparer<HashTableEntry>
    {
        public static readonly HashTableEntryFind Instance = new();

        #region Constructors

        private HashTableEntryFind()
        {
        }

        #endregion

        #region Implementation of IComparer<HashTableEntry>

        public int Compare(HashTableEntry x, HashTableEntry y)
        {
            return x.Hash.CompareTo(y.Hash);
        }

        #endregion
    }
}