using System.Runtime.CompilerServices;

namespace Difflytic.Hash.Adler32
{
    public sealed class Adler32RollingHash : IRollingHash
    {
        private const uint Mod = 65521;
        private readonly uint _pop;
        private readonly byte[] _window;
        private uint _a;
        private uint _b;
        private uint _index;
        private bool _isWindowFilled;

        #region Constructors

        public Adler32RollingHash(uint blockSize)
        {
            _a = 1;
            _pop = Mod - blockSize;
            _window = new byte[blockSize];
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Pop(byte value)
        {
            _a = (_a + Mod - value) % Mod;
            _b = (_b + Mod - 1 + _pop * value) % Mod;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Push(byte value)
        {
            _a = (_a + value) % Mod;
            _b = (_b + _a) % Mod;
        }

        #endregion

        #region Implementation of IRollingHash

        public void Add(byte value)
        {
            if (_isWindowFilled)
            {
                Pop(_window[_index]);
            }

            _window[_index] = value;
            _index++;

            if (_index == _window.Length)
            {
                _index = 0;
                _isWindowFilled = true;
            }

            Push(value);
        }

        public uint Digest()
        {
            return (_b << 16) | _a;
        }

        #endregion
    }
}