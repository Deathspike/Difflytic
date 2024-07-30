using System;
using System.IO;

namespace Difflytic
{
    public static class BlockSize
    {
        private const uint MaximumHashTableSize = 6553600;
        private const uint MinimumBlockSize = 16;

        #region Statics

        public static int GetSize(string oldPath)
        {
            return (int)Math.Max(MinimumBlockSize, new FileInfo(oldPath).Length / MaximumHashTableSize);
        }

        #endregion
    }
}