using System;
using System.IO;
using System.Text;

namespace Difflytic.Patching.Reading.IO
{
    public sealed class DiffStreamHeader
    {
        private readonly DiffStreamHeaderEntry[] _entries;

        #region Constructors

        private DiffStreamHeader(long headerCount)
        {
            _entries = new DiffStreamHeaderEntry[headerCount];
        }

        public static DiffStreamHeader Create(long dataPosition, Stream diffStream, long headerCount, long headerPosition)
        {
            var fileHeader = new DiffStreamHeader(headerCount);
            diffStream.Position = headerPosition;
            fileHeader.Init(dataPosition, diffStream);
            return fileHeader;
        }

        #endregion

        #region Methods

        public DiffStreamHeaderEntry? Find(long localPosition)
        {
            var index = Array.BinarySearch(_entries, new DiffStreamHeaderEntry { LocalPosition = localPosition }, DiffStreamHeaderEntryFind.Instance);
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