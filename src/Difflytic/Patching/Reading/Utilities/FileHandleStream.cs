using System;
using System.IO;
using Microsoft.Win32.SafeHandles;

namespace Difflytic.Patching.Reading.Utilities
{
    internal sealed class FileHandleStream : Stream
    {
        private readonly SafeFileHandle _diffHandle;
        private readonly bool _ownsHandle;
        private long _position;

        #region Constructors

        public FileHandleStream(SafeFileHandle diffHandle, bool ownsHandle = true)
        {
            _diffHandle = diffHandle;
            _ownsHandle = ownsHandle;
        }

        #endregion

        #region Destructors

        protected override void Dispose(bool disposing)
        {
            if (disposing && _ownsHandle)
            {
                _diffHandle.Dispose();
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
            var bytesRead = RandomAccess.Read(_diffHandle, new Span<byte>(buffer, offset, count), _position);
            if (bytesRead == 0) return 0;
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
                    throw new NotSupportedException();
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
            get => throw new NotSupportedException();
        }

        public override long Position
        {
            get => _position;
            set => _position = value;
        }

        #endregion
    }
}