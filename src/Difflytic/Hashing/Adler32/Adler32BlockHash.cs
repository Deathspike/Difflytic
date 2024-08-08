using Difflytic.Hashing.Abstracts;

namespace Difflytic.Hashing.Adler32
{
    public sealed class Adler32BlockHash : IBlockHash
    {
        #region Implementation of IBlockHash

        public uint AddAndDigest(byte[] buffer, int count)
        {
            var a = 1u;
            var b = 0u;

            for (var i = 0; i < count; i++)
            {
                a += buffer[i];
                b += a;

                if (a >= Adler32.Mod) a -= Adler32.Mod;
                if (b >= Adler32.Mod) b -= Adler32.Mod;
            }

            return (b << 16) | a;
        }

        #endregion
    }
}