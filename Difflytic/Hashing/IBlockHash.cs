using System;

namespace Difflytic.Hashing
{
    public interface IBlockHash
    {
        #region Methods

        uint AddAndDigest(Span<byte> buffer);

        #endregion
    }
}