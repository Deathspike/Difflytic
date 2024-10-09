using System;
using System.IO;

namespace Difflytic.Patching.Reading.IO
{
    public sealed class DiffStream : Stream
    {
        private readonly Stream _diffStream;
        private readonly DiffStreamHeader _header;
        private readonly Stream _oldStream;
        private long _position;

        #region Constructors

        public DiffStream(Stream diffStream, DiffStreamHeader header, Stream oldStream)
        {
            _diffStream = diffStream;
            _header = header;
            _oldStream = oldStream;
        }

        #endregion

        #region Destructors

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _diffStream.Dispose();
                _oldStream.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Methods

        private int ReadData(byte[] buffer, int count, int offset, long position)
        {
            _diffStream.Position = position;
            var bytesRead = _diffStream.Read(buffer, offset, count);
            if (bytesRead == 0) return 0;
            Position += bytesRead;
            return bytesRead;
        }

        private int ReadReference(byte[] buffer, int count, int offset, long position)
        {
            _oldStream.Position = position;
            var bytesRead = _oldStream.Read(buffer, offset, count);
            if (bytesRead == 0) return 0;
            Position += bytesRead;
            return bytesRead;
        }

        #endregion

        #region Overrides of Stream

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            // Find the block for the position.
            var block = _header.Find(_position);
            if (block == null) return 0;

            // Find the offset within the block.
            var blockOffset = _position - block.Value.LocalPosition;
            var maxBytesToRead = block.Value.Length - blockOffset;
            if (maxBytesToRead == 0) return 0;

            // Read the requested sequence of bytes.
            var maxCount = maxBytesToRead < int.MaxValue ? Math.Min(count, (int)maxBytesToRead) : count;
            var position = block.Value.OtherPosition + blockOffset;
            return block.Value.InData ? ReadData(buffer, maxCount, offset, position) : ReadReference(buffer, maxCount, offset, position);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    break;
                case SeekOrigin.End:
                    Position = Length - offset;
                    break;
            }

            return _position;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead
        {
            get => true;
        }

        public override bool CanSeek
        {
            get => true;
        }

        public override bool CanWrite
        {
            get => false;
        }

        public override long Length
        {
            get => _header.Length;
        }

        public override long Position
        {
            get => _position;
            set => _position = Math.Max(0, Math.Min(value, Length));
        }

        #endregion
    }
}