using System;
using System.Collections.Generic;
using System.IO;
using Difflytic.Hash;

namespace Difflytic.Match
{
    public sealed class HashTable
    {
        private readonly IBlockHash _blockHash;
        private readonly uint _blockSize;
        private readonly uint _numberOfBlocks;
        private readonly HashTableEntry[] _values;

        #region Constructors

        private HashTable(IBlockHash blockHash, Options options)
        {
            _blockHash = blockHash;
            _blockSize = options.BlockSize;
            _numberOfBlocks = options.NumberOfBlocks;
            _values = new HashTableEntry[options.NumberOfBlocks];
        }

        public static HashTable Create(IBlockHash blockHash, Options options, Stream stream)
        {
            var hashTable = new HashTable(blockHash, options);
            hashTable.Init(stream);
            return hashTable;
        }

        #endregion

        #region Methods

        public IEnumerable<long>? Find(uint hash)
        {
            var index = Array.BinarySearch(_values, new HashTableEntry { Hash = hash }, HashTableEntryFind.Instance);
            if (index <= 0) return null;
            var result = new List<long>();

            while (index > 0)
            {
                ref var entry = ref _values[index - 1];
                if (entry.Hash != hash) break;
                index--;
            }

            while (index < _numberOfBlocks)
            {
                ref var entry = ref _values[index];
                if (entry.Hash != hash) break;
                result.Add(entry.Position);
                index++;
            }

            return result;
        }

        private void Init(Stream stream)
        {
            var buffer = new byte[_blockSize];
            var position = 0L;

            for (var i = 0; i < _numberOfBlocks; i++)
            {
                stream.ReadExactly(buffer);
                ref var entry = ref _values[i];
                entry.Hash = _blockHash.AddAndDigest(buffer);
                entry.Position = position;
                position += _blockSize;
            }

            Array.Sort(_values, HashTableEntrySort.Instance);
        }

        #endregion
    }
}