using System;

namespace Difflytic.Hash
{
    public interface IBlockHash
    {
        #region Methods

        uint AddAndDigest(Span<byte> buffer);

        #endregion
    }
}