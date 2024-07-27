using System;
using System.Collections.Generic;
using System.IO;
using Difflytic.Diffing.Hashing;

namespace Difflytic.Diffing.Matching
{
    public sealed class HashTable
    {
        private readonly HashTableEntry[] _entries;

        #region Constructors

        private HashTable(int numberOfBlocks)
        {
            _entries = new HashTableEntry[numberOfBlocks];
        }

        public static HashTable Create(IBlockHash blockHash, int blockSize, int numberOfBlocks, Stream stream)
        {
            var hashTable = new HashTable(numberOfBlocks);
            hashTable.Init(blockHash, blockSize, stream);
            return hashTable;
        }

        #endregion

        #region Methods

        public IEnumerable<long>? Find(uint hash)
        {
            var index = Array.BinarySearch(_entries, new HashTableEntry { Hash = hash }, HashTableEntryFind.Instance);
            if (index < 0) return null;
            var result = new List<long>();

            while (index > 0)
            {
                ref var entry = ref _entries[index - 1];
                if (entry.Hash != hash) break;
                index--;
            }

            while (index < _entries.Length)
            {
                ref var entry = ref _entries[index];
                if (entry.Hash != hash) break;
                result.Add(entry.Position);
                index++;
            }

            return result;
        }

        private void Init(IBlockHash blockHash, int blockSize, Stream stream)
        {
            var buffer = new byte[blockSize];
            var position = 0L;

            for (var i = 0; i < _entries.Length; i++)
            {
                stream.ReadExactly(buffer);
                ref var entry = ref _entries[i];
                entry.Hash = blockHash.AddAndDigest(buffer);
                entry.Position = position;
                position += blockSize;
            }

            Array.Sort(_entries, HashTableEntrySort.Instance);
        }

        #endregion
    }
}