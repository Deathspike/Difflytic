using System;
using System.IO;
using System.IO.Compression;
using Difflytic.Patching.Reading.IO;

namespace Difflytic.Patching.Reading
{
    public static class ReaderExtensions
    {
        #region Statics

        public static Stream Open(this Reader reader, ReaderFile file)
        {
            switch (file.Type)
            {
                case FileType.Diff:
                {
                    var diffHandle = reader.DiffHandle;
                    var oldHandle = reader.OldHandle;
                    return new DiffStream(diffHandle, DiffStreamHeader.Create(file.DataPosition, diffHandle, file.HeaderCount, file.HeaderPosition), oldHandle);
                }
                case FileType.Raw:
                {
                    var diffHandle = reader.DiffHandle;
                    return new RawStream(diffHandle, file);
                }
                case FileType.RawGZip:
                {
                    var diffHandle = reader.DiffHandle;
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