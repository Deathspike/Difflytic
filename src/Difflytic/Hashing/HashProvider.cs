using System;
using Difflytic.Hashing.Adler32;

namespace Difflytic.Hashing
{
    public static class HashProvider
    {
        #region Constructors

        public static IBlockHash CreateBlockHash(HashType type)
        {
            switch (type)
            {
                case HashType.Adler32:
                    return new Adler32BlockHash();
                default:
                    throw new Exception(nameof(CreateBlockHash));
            }
        }

        public static IRollingHash CreateRollingHash(int blockSize, HashType type)
        {
            switch (type)
            {
                case HashType.Adler32:
                    return new Adler32RollingHash(blockSize);
                default:
                    throw new Exception(nameof(CreateBlockHash));
            }
        }

        #endregion
    }
}