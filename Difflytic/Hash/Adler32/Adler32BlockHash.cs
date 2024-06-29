using System;

namespace Difflytic.Hash.Adler32
{
    public sealed class Adler32BlockHash : IBlockHash
    {
        private const uint Mod = 65521;
        private uint _a;
        private uint _b;

        #region Constructors

        public Adler32BlockHash()
        {
            _a = 1;
        }

        #endregion

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
                _a = (_a + value) % Mod;
                _b = (_b + _a) % Mod;
            }

            return Digest();
        }

        #endregion
    }
}