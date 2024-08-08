namespace Difflytic.Hashing.Abstracts
{
    public interface IRollingHash
    {
        #region Methods

        void Add(byte value);

        uint Digest();

        #endregion
    }
}