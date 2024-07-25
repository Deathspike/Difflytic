using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Difflytic.Extensions;
using Difflytic.Hashing;
using Difflytic.Matching;

namespace Difflytic
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

        public void Diff(string oldPath, params string[] newPaths)
        {
            var hashTable = CreateHashTable(oldPath);
            WriteFiles(oldPath, newPaths, hashTable);
            MergeFiles(oldPath, newPaths);
            File.Move(oldPath + ".diffl.tmp", oldPath + ".diffl", true);
        }

        private HashTable CreateHashTable(string oldPath)
        {
            using var oldFileStream = new BufferedStream(File.OpenRead(oldPath));
            var blockHash = _hashFactory.CreateBlockHash();
            return HashTable.Create(blockHash, _blockSize, _numberOfBlocks, oldFileStream);
        }

        private void WriteFiles(string oldPath, IEnumerable<string> newPaths, HashTable hashTable)
        {
            Parallel.ForEach(newPaths, newPath =>
            {
                using var newFileStream = new BufferedStream(File.OpenRead(newPath));
                using var oldFileStream = new BufferedStream(File.OpenRead(oldPath));
                using var headerStream = new BufferedStream(File.OpenWrite(newPath + ".diffh.tmp"));
                using var headerWriter = new BinaryWriter(headerStream);
                using var dataStream = new BufferedStream(File.OpenWrite(newPath + ".diffd.tmp"));
                headerStream.SetLength(0);
                dataStream.SetLength(0);

                foreach (var block in new Matcher(_blockSize, hashTable, newFileStream, oldFileStream, _hashFactory.CreateRollingHash(_blockSize)))
                {
                    if (block.IsCopy)
                    {
                        headerWriter.Write(true);
                        headerWriter.Write7BitEncodedInt64(block.Length);
                        newFileStream.Position = block.NewPosition;
                        newFileStream.CopyExactly(dataStream, block.Length);
                    }
                    else
                    {
                        headerWriter.Write(false);
                        headerWriter.Write7BitEncodedInt64(block.Length);
                        headerWriter.Write7BitEncodedInt64(block.OldPosition);
                    }
                }
            });
        }

        #endregion

        #region Statics

        private static void MergeFiles(string oldPath, IReadOnlyCollection<string> newPaths)
        {
            using var outputStream = new BufferedStream(File.OpenWrite(oldPath + ".diffl.tmp"));
            using var outputWriter = new BinaryWriter(outputStream);
            outputStream.SetLength(0);
            outputWriter.Write("difflytic"u8);
            outputWriter.Write((byte)1);
            outputWriter.Write7BitEncodedInt(newPaths.Count);

            foreach (var newPath in newPaths)
            {
                outputWriter.Write(Path.GetFileName(newPath));
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

        #endregion
    }
}