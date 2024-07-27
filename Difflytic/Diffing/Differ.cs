﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Difflytic.Diffing.Hashing;
using Difflytic.Diffing.Matching;
using Difflytic.Extensions;

namespace Difflytic.Diffing
{
    public sealed class Differ
    {
        private readonly int _blockSize;
        private readonly IHashFactory _hashFactory;
        private readonly int _numberOfBlocks;

        #region Constructors

        public Differ(int blockSize, IHashFactory hashFactory, int numberOfBlocks)
        {
            _blockSize = blockSize;
            _hashFactory = hashFactory;
            _numberOfBlocks = numberOfBlocks;
        }

        #endregion

        #region Methods

        public void Diff(string diffPath, string[] newPaths, string oldPath)
        {
            var hashTable = CreateHashTable(oldPath);
            var headerCounts = new ConcurrentDictionary<string, long>();
            WriteFiles(hashTable, headerCounts, newPaths, oldPath);
            MergeFiles(headerCounts, newPaths, oldPath);
            File.Move(oldPath + ".diffl.tmp", diffPath, true);
        }

        private HashTable CreateHashTable(string oldPath)
        {
            using var oldStream = new BufferedStream(File.OpenRead(oldPath));
            var blockHash = _hashFactory.CreateBlockHash();
            return HashTable.Create(blockHash, _blockSize, _numberOfBlocks, oldStream);
        }

        private void MergeFiles(ConcurrentDictionary<string, long> headerCounts, IReadOnlyCollection<string> newPaths, string oldPath)
        {
            using var outputStream = new BufferedStream(File.OpenWrite(oldPath + ".diffl.tmp"));
            using var outputWriter = new BinaryWriter(outputStream);
            outputStream.SetLength(0);
            outputWriter.Write("difflytic"u8);
            outputWriter.Write((byte)1);
            outputWriter.Write7BitEncodedInt(newPaths.Count);

            foreach (var newPath in newPaths)
            {
                if (!headerCounts.TryGetValue(newPath, out var headerCount)) throw new Exception(nameof(MergeFiles));
                outputWriter.Write(Path.GetFileName(newPath));
                outputWriter.Write7BitEncodedInt64(headerCount);
                outputWriter.Write7BitEncodedInt64(new FileInfo(newPath + ".diffh.tmp").Length);
                outputWriter.Write7BitEncodedInt64(new FileInfo(newPath + ".diffd.tmp").Length);
            }

            foreach (var newPath in newPaths)
            {
                using var headerStream = new BufferedStream(File.OpenRead(newPath + ".diffh.tmp"));
                using var dataStream = new BufferedStream(File.OpenRead(newPath + ".diffd.tmp"));
                headerStream.CopyTo(outputStream);
                dataStream.CopyTo(outputStream);
            }

            foreach (var newPath in newPaths)
            {
                File.Delete(newPath + ".diffh.tmp");
                File.Delete(newPath + ".diffd.tmp");
            }
        }

        private void WriteFiles(HashTable hashTable, ConcurrentDictionary<string, long> headerCounts, IEnumerable<string> newPaths, string oldPath)
        {
            Parallel.ForEach(newPaths, newPath =>
            {
                // Configure the matcher.
                using var newStream = new BufferedStream(File.OpenRead(newPath));
                using var oldStream = new BufferedStream(File.OpenRead(oldPath));
                var headerCount = 0;
                var matcher = new Matcher(_blockSize, hashTable, newStream, oldStream, _hashFactory.CreateRollingHash(_blockSize));

                // Configure the data and header streams.
                using var dataStream = new BufferedStream(File.OpenWrite(newPath + ".diffd.tmp"));
                using var headerStream = new BufferedStream(File.OpenWrite(newPath + ".diffh.tmp"));
                using var headerWriter = new BinaryWriter(headerStream);
                dataStream.SetLength(0);
                headerStream.SetLength(0);

                // Match the blocks and write the streams.
                foreach (var block in matcher)
                {
                    if (block.IsCopy)
                    {
                        headerWriter.Write(true);
                        headerWriter.Write7BitEncodedInt64(block.Length);
                        newStream.Position = block.NewPosition;
                        newStream.CopyExactly(dataStream, block.Length);
                    }
                    else
                    {
                        headerWriter.Write(false);
                        headerWriter.Write7BitEncodedInt64(block.Length);
                        headerWriter.Write7BitEncodedInt64(block.OldPosition);
                    }

                    headerCount++;
                }

                // Save the header count for the entry table.
                if (headerCounts.TryAdd(newPath, headerCount)) return;
                throw new Exception(nameof(WriteFiles));
            });
        }

        #endregion
    }
}