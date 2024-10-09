using System;
using System.IO;
using System.IO.Compression;
using Difflytic.Patching.Reading.IO;

namespace Difflytic.Patching.Reading
{
    public static class ReaderFileExtensions
    {
        #region Statics

        public static Stream Open(this ReaderFile file, string diffPath, string oldPath)
        {
            switch (file.Type)
            {
                case FileType.Diff:
                {
                    var diffHandle = File.OpenHandle(diffPath);
                    var header = DiffStreamHeader.Create(file.DataPosition, diffHandle, file.HeaderCount, file.HeaderPosition);
                    var fileHandle = File.OpenHandle(oldPath);
                    return new DiffStream(diffHandle, header, fileHandle);
                }
                case FileType.Raw:
                {
                    var diffHandle = File.OpenHandle(diffPath);
                    return new RawStream(diffHandle, file);
                }
                case FileType.RawGZip:
                {
                    var diffHandle = File.OpenHandle(diffPath);
                    var rawStream = new RawStream(diffHandle, file);
                    return new GZipStream(rawStream, CompressionMode.Decompress);
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(Open));
                }
            }
        }

        #endregion
    }
}