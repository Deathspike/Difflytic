using System;

namespace Difflytic.Hash.Adler32
{
    public sealed class Adler32BlockHash : IBlockHash
    {
        private const uint Mod = 65521;
        private uint _a = 1;
        private uint _b;

        #region Methods

        private uint Digest()
        {
            var hash = (_b << 16) | _a;
            _a = 1;
            _b = 0;
            return hash;
        }

        #endregion

        #region Implementation of IBlockHash

        public uint AddAndDigest(Span<byte> buffer)
        {
            foreach (var value in buffer)
            {
                _a += value;
                _b += _a;

                while (_a >= Mod) _a -= Mod;
                while (_b >= Mod) _b -= Mod;
            }

            return Digest();
        }

        #endregion
    }
}