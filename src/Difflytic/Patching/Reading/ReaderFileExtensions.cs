using System;
using System.IO;
using System.IO.Compression;
using Difflytic.Patching.Reading.IO;
using Microsoft.Win32.SafeHandles;

namespace Difflytic.Patching.Reading
{
    public static class ReaderFileExtensions
    {
        #region Statics

        public static Stream Open(this ReaderFile file, SafeFileHandle diffHandle, SafeFileHandle oldHandle)
        {
            switch (file.Type)
            {
                case FileType.Diff:
                {
                    return new DiffStream(diffHandle, DiffStreamHeader.Create(file.DataPosition, diffHandle, file.HeaderCount, file.HeaderPosition), oldHandle);
                }
                case FileType.Raw:
                {
                    return new RawStream(diffHandle, file);
                }
                case FileType.RawGZip:
                {
                    return new GZipStream(new RawStream(diffHandle, file), CompressionMode.Decompress);
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(file.Type.ToString());
                }
            }
        }

        #endregion
    }
}