namespace Difflytic.Hash
{
    public interface IRollingHash
    {
        #region Methods

        void Add(byte value);

        uint Digest();

        #endregion
    }
}