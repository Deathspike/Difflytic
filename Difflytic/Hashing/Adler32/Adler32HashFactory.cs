namespace Difflytic.Hashing.Adler32
{
    public sealed class Adler32HashFactory : IHashFactory
    {
        public static readonly Adler32HashFactory Instance = new();

        #region Constructors

        private Adler32HashFactory()
        {
        }

        #endregion

        #region Implementation of IHashFactory

        public IBlockHash CreateBlockHash()
        {
            return new Adler32BlockHash();
        }

        public IRollingHash CreateRollingHash(int blockSize)
        {
            return new Adler32RollingHash(blockSize);
        }

        #endregion
    }
}