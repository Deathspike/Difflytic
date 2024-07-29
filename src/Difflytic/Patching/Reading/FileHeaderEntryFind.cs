using System.Collections.Generic;

namespace Difflytic.Patching.Reading
{
    public sealed class FileHeaderEntryFind : IComparer<FileHeaderEntry>
    {
        public static readonly FileHeaderEntryFind Instance = new();

        #region Constructors

        private FileHeaderEntryFind()
        {
        }

        #endregion

        #region Implementation of IComparer<FileHeaderEntry>

        public int Compare(FileHeaderEntry x, FileHeaderEntry y)
        {
            if (y.LocalPosition >= x.LocalPosition && y.LocalPosition < x.LocalPosition + x.Length) return 0;
            if (y.LocalPosition > x.LocalPosition) return -1;
            return 1;
        }

        #endregion
    }
}