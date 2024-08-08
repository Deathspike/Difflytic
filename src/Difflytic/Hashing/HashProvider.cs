using System;
using Difflytic.Hashing.Abstracts;
using Difflytic.Hashing.Adler32;
using Difflytic.Hashing.MLCG;

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
                case HashType.MLCG:
                    return new MLCGBlockHash();
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
                case HashType.MLCG:
                    return new MLCGRollingHash(blockSize);
                default:
                    throw new Exception(nameof(CreateBlockHash));
            }
        }

        #endregion
    }
}