using System;
using System.IO;
using Microsoft.Win32.SafeHandles;

namespace Difflytic.Patching.Reading.Utilities
{
    public sealed class FileHandleStream : Stream
    {
        private readonly SafeFileHandle _diffHandle;
        private long _position;

        #region Constructors

        public FileHandleStream(SafeFileHandle diffHandle)
        {
            _diffHandle = diffHandle;
        }

        #endregion

        #region Overrides of Stream

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var bytesRead = RandomAccess.Read(_diffHandle, new Span<byte>(buffer, offset, count), _position);
            _position += bytesRead;
            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                {
                    Position = offset;
                    break;
                }
                case SeekOrigin.Current:
                {
                    Position += offset;
                    break;
                }
                case SeekOrigin.End:
                {
                    Position = Length - offset;
                    break;
                }
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
            get => RandomAccess.GetLength(_diffHandle);
        }

        public override long Position
        {
            get => _position;
            set => _position = Math.Max(0, Math.Min(value, Length));
        }

        #endregion
    }
}