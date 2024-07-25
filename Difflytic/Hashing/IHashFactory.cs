namespace Difflytic.Hashing
{
    public interface IHashFactory
    {
        #region Methods

        IBlockHash CreateBlockHash();

        IRollingHash CreateRollingHash(int blockSize);

        #endregion
    }
}