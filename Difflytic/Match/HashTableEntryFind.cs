﻿using System.Collections.Generic;

namespace Difflytic.Match
{
    public sealed class HashTableEntryFind : IComparer<HashTableEntry>
    {
        public static HashTableEntryFind Instance = new();

        #region Implementation of IComparer<HashTableEntry>

        public int Compare(HashTableEntry x, HashTableEntry y)
        {
            return x.Hash.CompareTo(y.Hash);
        }

        #endregion
    }
}