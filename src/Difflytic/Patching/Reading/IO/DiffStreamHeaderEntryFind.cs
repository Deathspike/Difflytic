using System.Collections.Generic;

namespace Difflytic.Patching.Reading.IO
{
    public sealed class DiffStreamHeaderEntryFind : IComparer<DiffStreamHeaderEntry>
    {
        public static readonly DiffStreamHeaderEntryFind Instance = new();

        #region Constructors

        private DiffStreamHeaderEntryFind()
        {
        }

        #endregion

        #region Implementation of IComparer<FileHeaderEntry>

        public int Compare(DiffStreamHeaderEntry x, DiffStreamHeaderEntry y)
        {
            if (y.LocalPosition >= x.LocalPosition && y.LocalPosition < x.LocalPosition + x.Length) return 0;
            if (y.LocalPosition > x.LocalPosition) return -1;
            return 1;
        }

        #endregion
    }
}