namespace Difflytic.Hashing.Abstracts
{
    public interface IBlockHash
    {
        #region Methods

        uint AddAndDigest(byte[] buffer, int count);

        #endregion
    }
}