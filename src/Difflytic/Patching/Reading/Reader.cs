using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Difflytic.Hashing;
using Difflytic.Patching.Reading.Utilities;
using Microsoft.Win32.SafeHandles;

namespace Difflytic.Patching.Reading
{
    public sealed class Reader : List<ReaderFile>
    {
        #region Constructors

        private Reader(SafeFileHandle diffHandle, SafeFileHandle oldHandle)
        {
            DiffHandle = diffHandle;
            OldHandle = oldHandle;
        }

        public static Reader Create(SafeFileHandle diffHandle, SafeFileHandle oldHandle, bool skipValidate = false)
        {
            var reader = new Reader(diffHandle, oldHandle);
            reader.Init(skipValidate);
            return reader;
        }

        #endregion

        #region Methods

        private void Init(bool skipValidate)
        {
            // Configure the file stream and reader.
            using var diffStream = new BufferedStream(new FileHandleStream(DiffHandle));
            using var diffReader = new BinaryReader(diffStream, Encoding.UTF8, true);

            // Validate the file signature and version.
            var signature = Encoding.ASCII.GetString(diffReader.ReadBytes(9));
            var version = diffReader.ReadByte();
            var hashType = diffReader.ReadByte();
            if (signature != "difflytic" || version > 2 || !Enum.IsDefined(typeof(HashType), hashType)) throw new Exception(nameof(Init));

            // Validate the file hash.
            var blockSize = diffReader.Read7BitEncodedInt();
            var fullHash = diffReader.ReadUInt32();
            if (!skipValidate) ValidateFile(blockSize, fullHash, hashType);

            // Read the files.
            ReadFiles(diffReader, version);
            UpdateFiles(diffStream);
        }

        private void ReadFiles(BinaryReader diffReader, byte version)
        {
            for (var i = diffReader.Read7BitEncodedInt(); i > 0; i--)
            {
                var name = diffReader.ReadString();
                var type = version >= 2 ? (FileType)diffReader.ReadByte() : FileType.Diff;

                switch (type)
                {
                    case FileType.Diff:
                    {
                        var headerCount = diffReader.Read7BitEncodedInt64();
                        var headerLength = diffReader.Read7BitEncodedInt64();
                        var dataLength = diffReader.Read7BitEncodedInt64();
                        Add(new ReaderFile(dataLength, headerCount, headerLength, name, type));
                        break;
                    }
                    default:
                    {
                        var dataLength = diffReader.Read7BitEncodedInt64();
                        Add(new ReaderFile(dataLength, 0, 0, name, type));
                        break;
                    }
                }
            }
        }

        private void UpdateFiles(Stream diffStream)
        {
            foreach (var file in this)
            {
                file.HeaderPosition = diffStream.Position;
                diffStream.Position += file.HeaderLength;
                file.DataPosition = diffStream.Position;
                diffStream.Position += file.DataLength;
            }
        }

        private void ValidateFile(int blockSize, uint fullHash, byte hashType)
        {
            using var oldStream = new BufferedStream(new FileHandleStream(OldHandle));
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

        #region Properties

        public SafeFileHandle DiffHandle { get; }
        public SafeFileHandle OldHandle { get; }

        #endregion
    }
}