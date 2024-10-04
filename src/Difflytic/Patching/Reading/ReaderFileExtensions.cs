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
                    var diffStream = new BufferedStream(File.OpenRead(diffPath));
                    var oldStream = new BufferedStream(File.OpenRead(oldPath));
                    return DiffStream.Create(diffStream, file, oldStream);
                }
                case FileType.Raw:
                {
                    var diffStream = new BufferedStream(File.OpenRead(diffPath));
                    return new RawStream(diffStream, file);
                }
                case FileType.RawGZip:
                {
                    var diffStream = new BufferedStream(File.OpenRead(diffPath));
                    var rawStream = new RawStream(diffStream, file);
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