using System;
using System.IO;
using System.Text;

namespace Difflytic.Patching.Reading
{
    public sealed class FileHeader
    {
        private readonly FileHeaderEntry[] _entries;

        #region Constructors

        private FileHeader(long headerCount)
        {
            _entries = new FileHeaderEntry[headerCount];
        }

        public static FileHeader Create(long dataPosition, Stream diffStream, long headerCount, long headerPosition)
        {
            var fileHeader = new FileHeader(headerCount);
            diffStream.Position = headerPosition;
            fileHeader.Init(dataPosition, diffStream);
            return fileHeader;
        }

        #endregion

        #region Methods

        public FileHeaderEntry? Find(long localPosition)
        {
            var index = Array.BinarySearch(_entries, new FileHeaderEntry { LocalPosition = localPosition }, FileHeaderEntryFind.Instance);
            if (index < 0) return null;
            return _entries[index];
        }

        private void Init(long dataPosition, Stream diffStream)
        {
            using var diffReader = new BinaryReader(diffStream, Encoding.UTF8, true);
            var dataOffset = 0L;

            for (var i = 0; i < _entries.Length; i++)
            {
                if (diffReader.ReadBoolean())
                {
                    ref var entry = ref _entries[i];
                    entry.InData = true;
                    entry.Length = diffReader.Read7BitEncodedInt64();
                    entry.LocalPosition = Length;
                    entry.OtherPosition = dataPosition + dataOffset;
                    dataOffset += entry.Length;
                    Length += entry.Length;
                }
                else
                {
                    ref var entry = ref _entries[i];
                    entry.Length = diffReader.Read7BitEncodedInt64();
                    entry.LocalPosition = Length;
                    entry.OtherPosition = diffReader.Read7BitEncodedInt64();
                    Length += entry.Length;
                }
            }
        }

        #endregion

        #region Properties

        public long Length { get; private set; }

        #endregion
    }
}