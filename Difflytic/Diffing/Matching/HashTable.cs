using System;
using System.Collections.Generic;
using System.IO;
using Difflytic.Hashing;

namespace Difflytic.Diffing.Matching
{
    public sealed class HashTable
    {
        private readonly HashTableEntry[] _entries;

        #region Constructors

        private HashTable(long numberOfBlocks)
        {
            _entries = new HashTableEntry[numberOfBlocks];
        }

        public static HashTable Create(IBlockHash blockHash, int blockSize, Stream oldStream)
        {
            var hashTable = new HashTable(oldStream.Length / blockSize);
            hashTable.Init(blockHash, blockSize, oldStream);
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

        private void Init(IBlockHash blockHash, int blockSize, Stream oldStream)
        {
            var buffer = new byte[blockSize];
            var position = 0L;

            for (var i = 0; i < _entries.Length; i++, position += blockSize)
            {
                oldStream.ReadExactly(buffer);
                ref var entry = ref _entries[i];
                entry.Hash = blockHash.AddAndDigest(buffer, blockSize);
                entry.Position = position;
                FullHash += entry.Hash;
            }

            while (oldStream.Position != oldStream.Length)
            {
                var count = (int)(oldStream.Length - oldStream.Position);
                oldStream.ReadExactly(buffer, 0, count);
                FullHash += blockHash.AddAndDigest(buffer, count);
            }

            Array.Sort(_entries, HashTableEntrySort.Instance);
        }

        #endregion

        #region Properties

        public uint FullHash { get; private set; }

        #endregion
    }
}