﻿using System;
using System.IO;
using Microsoft.Win32.SafeHandles;

namespace Difflytic.Patching.Reading.IO
{
    public sealed class RawStream : Stream
    {
        private readonly SafeFileHandle _diffHandle;
        private readonly ReaderFile _file;
        private readonly bool _ownsHandle;
        private long _position;

        #region Constructors

        public RawStream(SafeFileHandle diffHandle, ReaderFile file, bool ownsHandle = true)
        {
            _diffHandle = diffHandle;
            _file = file;
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
            // Find the maximum bytes.
            var maxBytesToRead = Length - _position;
            if (maxBytesToRead == 0) return 0;

            // Read the requested sequence of bytes.
            var maxCount = maxBytesToRead < int.MaxValue ? Math.Min(count, (int)maxBytesToRead) : count;
            var bytesRead = RandomAccess.Read(_diffHandle, new Span<byte>(buffer, offset, maxCount), _file.DataPosition + _position);
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