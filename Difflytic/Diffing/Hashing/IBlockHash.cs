using System;

namespace Difflytic.Diffing.Hashing
{
    public interface IBlockHash
    {
        #region Methods

        uint AddAndDigest(Span<byte> buffer);

        #endregion
    }
}