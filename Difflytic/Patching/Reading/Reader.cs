﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Difflytic.Hashing;

namespace Difflytic.Patching.Reading
{
    public sealed class Reader : IEnumerable<ReaderFile>
    {
        private readonly List<ReaderFile> _files;

        #region Constructors

        private Reader()
        {
            _files = new List<ReaderFile>();
        }

        public static Reader Create(string diffPath, string oldPath, bool skipValidate = false)
        {
            var reader = new Reader();
            reader.Init(diffPath, oldPath, skipValidate);
            return reader;
        }

        #endregion

        #region Methods

        private void Init(string diffPath, string oldPath, bool skipValidate)
        {
            // Configure the file stream and reader.
            using var diffStream = new BufferedStream(File.OpenRead(diffPath));
            using var diffReader = new BinaryReader(diffStream, Encoding.UTF8, true);

            // Validate the file signature and version.
            var signature = Encoding.ASCII.GetString(diffReader.ReadBytes(9));
            var version = diffReader.ReadByte();
            var hashType = diffReader.ReadByte();
            if (signature != "difflytic" || version != 1 || !Enum.IsDefined(typeof(HashType), hashType)) throw new Exception(nameof(Init));

            // Validate the file hash.
            var blockSize = diffReader.Read7BitEncodedInt();
            var fullHash = diffReader.ReadUInt32();
            if (!skipValidate) ValidateFile(blockSize, fullHash, hashType, oldPath);

            // Read the files.
            ReadFiles(diffReader);
            UpdateFiles(diffStream);
        }

        private void ReadFiles(BinaryReader diffReader)
        {
            for (var i = diffReader.Read7BitEncodedInt(); i > 0; i--)
            {
                var name = diffReader.ReadString();
                var headerCount = diffReader.Read7BitEncodedInt64();
                var headerLength = diffReader.Read7BitEncodedInt64();
                var dataLength = diffReader.Read7BitEncodedInt64();
                _files.Add(new ReaderFile(dataLength, headerCount, headerLength, name));
            }
        }

        private void UpdateFiles(Stream diffStream)
        {
            foreach (var file in _files)
            {
                file.HeaderPosition = diffStream.Position;
                diffStream.Position += file.HeaderLength;
                file.DataPosition = diffStream.Position;
                diffStream.Position += file.DataLength;
            }
        }

        private void ValidateFile(int blockSize, uint fullHash, byte hashType, string oldPath)
        {
            using var oldStream = new BufferedStream(File.OpenRead(oldPath));
            var buffer = new byte[blockSize];
            var blockHash = HashProvider.CreateBlockHash((HashType)hashType);
            var currentHash = 0U;

            for (var i = oldStream.Length / blockSize; i > 0; i--)
            {
                oldStream.ReadExactly(buffer);
                currentHash += blockHash.AddAndDigest(buffer, blockSize);
            }

            while (oldStream.Position != oldStream.Length)
            {
                var count = (int)(oldStream.Length - oldStream.Position);
                oldStream.ReadExactly(buffer, 0, count);
                currentHash += blockHash.AddAndDigest(buffer, count);
            }

            if (fullHash == currentHash) return;
            throw new Exception(nameof(ValidateFile));
        }

        #endregion

        #region Implementation of IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of IEnumerable<ReaderFile>

        public IEnumerator<ReaderFile> GetEnumerator()
        {
            return _files.GetEnumerator();
        }

        #endregion
    }
}