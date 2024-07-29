namespace Difflytic.Hashing
{
    public interface IBlockHash
    {
        #region Methods

        uint AddAndDigest(byte[] buffer, int count);

        #endregion
    }
}