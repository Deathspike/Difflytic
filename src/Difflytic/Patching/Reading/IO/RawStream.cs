using System;
using System.IO;

namespace Difflytic.Patching.Reading.IO
{
    public sealed class RawStream : Stream
    {
        private readonly Stream _diffStream;
        private readonly ReaderFile _file;
        private long _position;

        #region Constructors

        public RawStream(Stream diffStream, ReaderFile file)
        {
            _diffStream = diffStream;
            _file = file;
        }

        #endregion

        #region Destructors

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _diffStream.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Overrides of Stream

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            // Find the maximum bytes.
            var maxBytesToRead = Length - _position;
            if (maxBytesToRead == 0) return 0;

            // Read the requested sequence of bytes.
            _diffStream.Position = _file.DataPosition + _position;
            var maxCount = maxBytesToRead < int.MaxValue ? Math.Min(count, (int)maxBytesToRead) : count;
            var bytesRead = _diffStream.Read(buffer, offset, maxCount);
            if (bytesRead == 0) return 0;

            // Update the position.
            Position += bytesRead;
            return bytesRead;
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
            get => _file.DataLength;
        }

        public override long Position
        {
            get => _position;
            set => _position = Math.Max(0, Math.Min(value, Length));
        }

        #endregion
    }
}