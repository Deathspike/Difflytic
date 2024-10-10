using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
        private readonly HashType _hashType;

        #region Constructors

        public Differ(int blockSize, HashType hashType = HashType.MLCG)
        {
            _blockSize = blockSize;
            _hashType = hashType;
        }

        #endregion

        #region Methods

        public void Diff(string diffPath, IReadOnlyCollection<string> newPaths, string oldPath)
        {
            Diff(diffPath, newPaths.Select(newPath => new DifferFile(newPath, FileType.Diff)).ToList(), oldPath);
        }

        public void Diff(string diffPath, IReadOnlyCollection<DifferFile> files, string oldPath)
        {
            var hashTable = CreateHashTable(oldPath);
            var headerCounts = new ConcurrentDictionary<string, long>();
            WriteFiles(files, hashTable, headerCounts, oldPath);
            MergeFiles(files, hashTable, headerCounts, oldPath);
            File.Move(oldPath + ".diff.tmp", diffPath, true);
        }

        private HashTable CreateHashTable(string oldPath)
        {
            using var oldStream = new BufferedStream(File.OpenRead(oldPath));
            var blockHash = HashProvider.CreateBlockHash(_hashType);
            return HashTable.Create(blockHash, _blockSize, oldStream);
        }

        private void MergeFiles(IReadOnlyCollection<DifferFile> files, HashTable hashTable, ConcurrentDictionary<string, long> headerCounts, string oldPath)
        {
            using var outputStream = new BufferedStream(File.OpenWrite(oldPath + ".diff.tmp"));
            using var outputWriter = new BinaryWriter(outputStream, Encoding.UTF8, true);
            outputStream.SetLength(0);
            outputWriter.Write("difflytic"u8);
            outputWriter.Write((byte)2);
            outputWriter.Write((byte)_hashType);
            outputWriter.Write7BitEncodedInt(_blockSize);
            outputWriter.Write(hashTable.FullHash);
            outputWriter.Write7BitEncodedInt(files.Count);

            foreach (var file in files)
            {
                outputWriter.Write(Path.GetFileName(file.FilePath));
                outputWriter.Write((byte)file.Type);

                switch (file.Type)
                {
                    case FileType.Diff:
                    {
                        if (!headerCounts.TryGetValue(file.FilePath, out var headerCount)) throw new Exception(nameof(MergeFiles));
                        outputWriter.Write7BitEncodedInt64(headerCount);
                        outputWriter.Write7BitEncodedInt64(new FileInfo(file.FilePath + ".diffh.tmp").Length);
                        outputWriter.Write7BitEncodedInt64(new FileInfo(file.FilePath + ".diffd.tmp").Length);
                        break;
                    }
                    case FileType.Raw:
                    {
                        outputWriter.Write7BitEncodedInt64(new FileInfo(file.FilePath).Length);
                        break;
                    }
                    case FileType.RawGZip:
                    {
                        outputWriter.Write7BitEncodedInt64(new FileInfo(file.FilePath + ".gz.tmp").Length);
                        break;
                    }
                    default:
                    {
                        throw new ArgumentOutOfRangeException(file.Type.ToString());
                    }
                }
            }

            foreach (var file in files)
            {
                switch (file.Type)
                {
                    case FileType.Diff:
                    {
                        using var headerStream = new BufferedStream(File.OpenRead(file.FilePath + ".diffh.tmp"));
                        using var dataStream = new BufferedStream(File.OpenRead(file.FilePath + ".diffd.tmp"));
                        headerStream.CopyTo(outputStream);
                        dataStream.CopyTo(outputStream);
                        break;
                    }
                    case FileType.Raw:
                    {
                        using var dataStream = new BufferedStream(File.OpenRead(file.FilePath));
                        dataStream.CopyTo(outputStream);
                        break;
                    }
                    case FileType.RawGZip:
                    {
                        using var dataStream = new BufferedStream(File.OpenRead(file.FilePath + ".gz.tmp"));
                        dataStream.CopyTo(outputStream);
                        break;
                    }
                }
            }

            foreach (var file in files)
            {
                switch (file.Type)
                {
                    case FileType.Diff:
                    {
                        File.Delete(file.FilePath + ".diffh.tmp");
                        File.Delete(file.FilePath + ".diffd.tmp");
                        break;
                    }
                    case FileType.RawGZip:
                    {
                        File.Delete(file.FilePath + ".gz.tmp");
                        break;
                    }
                }
            }
        }

        private void WriteFiles(IReadOnlyCollection<DifferFile> files, HashTable hashTable, ConcurrentDictionary<string, long> headerCounts, string oldPath)
        {
            Parallel.ForEach(files, file =>
            {
                switch (file.Type)
                {
                    case FileType.Diff:
                    {
                        // Configure the file streams.
                        using var newStream = new BufferedStream(File.OpenRead(file.FilePath));
                        using var oldStream = new BufferedStream(File.OpenRead(oldPath));
                        var headerCount = 0;

                        // Configure the data and header streams.
                        using var dataStream = new BufferedStream(File.OpenWrite(file.FilePath + ".diffd.tmp"));
                        using var headerStream = new BufferedStream(File.OpenWrite(file.FilePath + ".diffh.tmp"));
                        using var headerWriter = new BinaryWriter(headerStream, Encoding.UTF8, true);
                        dataStream.SetLength(0);
                        headerStream.SetLength(0);

                        // Match the blocks.
                        foreach (var block in new Matcher(_blockSize, hashTable, newStream, oldStream, HashProvider.CreateRollingHash(_blockSize, _hashType)))
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
                        if (headerCounts.TryAdd(file.FilePath, headerCount)) break;
                        throw new Exception(nameof(WriteFiles));
                    }
                    case FileType.RawGZip:
                    {
                        using var dataStream = new BufferedStream(File.OpenWrite(file.FilePath + ".gz.tmp"));
                        using var compressStream = new GZipStream(dataStream, CompressionMode.Compress);
                        using var newStream = new BufferedStream(File.OpenRead(file.FilePath));
                        newStream.CopyTo(compressStream);
                        break;
                    }
                }
            });
        }

        #endregion
    }
}