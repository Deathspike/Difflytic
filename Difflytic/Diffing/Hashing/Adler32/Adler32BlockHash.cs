using System;

namespace Difflytic.Diffing.Hashing.Adler32
{
    public sealed class Adler32BlockHash : IBlockHash
    {
        #region Implementation of IBlockHash

        public uint AddAndDigest(Span<byte> buffer)
        {
            var a = 1u;
            var b = 0u;

            foreach (var value in buffer)
            {
                a += value;
                b += a;

                if (a >= Adler32.Mod) a -= Adler32.Mod;
                if (b >= Adler32.Mod) b -= Adler32.Mod;
            }

            return (b << 16) | a;
        }

        #endregion
    }
}