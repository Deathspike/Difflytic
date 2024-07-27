using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

        public static Reader Create(string diffPath)
        {
            var reader = new Reader();
            reader.Init(diffPath);
            return reader;
        }

        #endregion

        #region Methods

        private void Init(string diffPath)
        {
            // Configure the file stream and reader.
            using var diffStream = new BufferedStream(File.OpenRead(diffPath));
            using var diffReader = new BinaryReader(diffStream, Encoding.UTF8, true);

            // Validate the file signature and version.
            if (!diffReader.ReadBytes(9).SequenceEqual("difflytic"u8.ToArray())) throw new Exception(nameof(Init));
            var version = diffReader.ReadByte();
            if (version != 1) throw new Exception(nameof(Init));

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