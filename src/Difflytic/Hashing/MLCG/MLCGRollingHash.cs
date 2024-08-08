using System.Runtime.CompilerServices;
using Difflytic.Hashing.Abstracts;

namespace Difflytic.Hashing.MLCG
{
    public sealed class MLCGRollingHash : IRollingHash
    {
        private readonly byte[] _window;
        private uint _hash;
        private uint _index;
        private bool _isWindowFilled;

        #region Constructors

        public MLCGRollingHash(int blockSize)
        {
            _window = new byte[blockSize];
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Pop(byte value)
        {
            _hash -= value * MLCG.Powers[value];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Push(byte value)
        {
            _hash += value * MLCG.Powers[value];
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
            return _hash;
        }

        #endregion
    }
}