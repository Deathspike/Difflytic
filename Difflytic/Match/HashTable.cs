using System;
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

        private void Init(Stream stream)
        {
            var buffer = new byte[_blockSize];
            var position = 0ul;

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