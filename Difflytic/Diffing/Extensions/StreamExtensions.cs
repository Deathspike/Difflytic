using System.IO;

namespace Difflytic.Diffing.Extensions
{
    public static class StreamExtensions
    {
        #region Statics

        public static void CopyExactly(this Stream sourceStream, Stream destinationStream, long count)
        {
            for (var i = 0; i < count; i++)
            {
                var value = sourceStream.ReadByte();
                if (value == -1) throw new EndOfStreamException();
                destinationStream.WriteByte((byte)value);
            }
        }

        #endregion
    }
}