using System.Runtime.CompilerServices;

namespace Difflytic.Hash.Adler32
{
    public sealed class Adler32RollingHash : IRollingHash
    {
        private readonly uint _pop;
        private readonly byte[] _window;
        private uint _a;
        private uint _b;
        private uint _index;
        private bool _isWindowFilled;

        #region Constructors

        public Adler32RollingHash(int blockSize)
        {
            _a = 1;
            _pop = Adler32.Mod - (uint)blockSize;
            _window = new byte[blockSize];
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Pop(byte value)
        {
            _a = (_a + Adler32.Mod - value) % Adler32.Mod;
            _b = (_b + Adler32.Mod - 1 + _pop * value) % Adler32.Mod;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Push(byte value)
        {
            _a = (_a + value) % Adler32.Mod;
            _b = (_b + _a) % Adler32.Mod;
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