using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Difflytic.Diffing.Extensions;
using Difflytic.Diffing.Matching;
using Difflytic.Hashing;

namespace Difflytic.Diffing
{
    public sealed class Differ
    {
        private readonly int _blockSize;
        private readonly IHashFactory _hashFactory;

        #region Constructors

        public Differ(int blockSize, IHashFactory hashFactory)
        {
            _blockSize = blockSize;
            _hashFactory = hashFactory;
        }

        #endregion

        #region Methods

        public void Diff(string diffPath, string[] newPaths, string oldPath)
        {
            var hashTable = CreateHashTable(oldPath);
            var headerCounts = new ConcurrentDictionary<string, long>();
            WriteFiles(hashTable, headerCounts, newPaths, oldPath);
            MergeFiles(hashTable, headerCounts, newPaths, oldPath);
            File.Move(oldPath + ".diff.tmp", diffPath, true);
        }

        private HashTable CreateHashTable(string oldPath)
        {
            using var oldStream = new BufferedStream(File.OpenRead(oldPath));
            var blockHash = _hashFactory.CreateBlockHash();
            return HashTable.Create(blockHash, _blockSize, oldStream);
        }

        private void MergeFiles(HashTable hashTable, ConcurrentDictionary<string, long> headerCounts, IReadOnlyCollection<string> newPaths, string oldPath)
        {
            using var outputStream = new BufferedStream(File.OpenWrite(oldPath + ".diff.tmp"));
            using var outputWriter = new BinaryWriter(outputStream, Encoding.UTF8, true);
            outputStream.SetLength(0);
            outputWriter.Write("difflytic"u8);
            outputWriter.Write((byte)1);
            outputWriter.Write7BitEncodedInt(_blockSize);
            outputWriter.Write(hashTable.FullHash);
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
                // Configure the file streams.
                using var newStream = new BufferedStream(File.OpenRead(newPath));
                using var oldStream = new BufferedStream(File.OpenRead(oldPath));
                var headerCount = 0;

                // Configure the data and header streams.
                using var dataStream = new BufferedStream(File.OpenWrite(newPath + ".diffd.tmp"));
                using var headerStream = new BufferedStream(File.OpenWrite(newPath + ".diffh.tmp"));
                using var headerWriter = new BinaryWriter(headerStream, Encoding.UTF8, true);
                dataStream.SetLength(0);
                headerStream.SetLength(0);

                // Match the blocks.
                foreach (var block in new Matcher(_blockSize, hashTable, newStream, oldStream, _hashFactory.CreateRollingHash(_blockSize)))
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

                // Save the header count.
                if (headerCounts.TryAdd(newPath, headerCount)) return;
                throw new Exception(nameof(WriteFiles));
            });
        }

        #endregion
    }
}