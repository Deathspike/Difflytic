using Difflytic.Hashing.Abstracts;

namespace Difflytic.Hashing.MLCG
{
    public sealed class MLCGBlockHash : IBlockHash
    {
        #region Implementation of IBlockHash

        public uint AddAndDigest(byte[] buffer, int count)
        {
            var hash = 0u;

            for (var i = 0; i < count; i++)
            {
                hash += buffer[i] * MLCG.Powers[buffer[i]];
            }

            return hash;
        }

        #endregion
    }
}