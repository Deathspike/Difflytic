using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Difflytic.Hashing;

namespace Difflytic.Matching
{
    public sealed class Matcher : IEnumerable<MatcherBlock>
    {
        private readonly int _blockSize;
        private readonly HashTable _hashTable;
        private readonly Stream _newStream;
        private readonly Stream _oldStream;
        private readonly IRollingHash _rollingHash;

        #region Constructors

        private Matcher(int blockSize, HashTable hashTable, Stream newStream, Stream oldStream, IRollingHash rollingHash)
        {
            _blockSize = blockSize;
            _hashTable = hashTable;
            _newStream = newStream;
            _oldStream = oldStream;
            _rollingHash = rollingHash;
        }

        public static Matcher Create(int blockSize, HashTable hashTable, Stream newStream, Stream oldStream, IRollingHash rollingHash)
        {
            return new Matcher(blockSize, hashTable, newStream, oldStream, rollingHash);
        }

        #endregion

        #region Methods

        private MatcherBlock? GetBestBlock(long lastPosition, IEnumerable<long> oldStartPositions, long newStartPosition)
        {
            MatcherBlock? result = null;

            foreach (var oldStartPosition in oldStartPositions)
            {
                var block = GetBlock(lastPosition, oldStartPosition, newStartPosition);

                if (block == null)
                {
                    continue;
                }

                if (result == null || block.Length > result.Length || (block.Length == result.Length && block.OldPosition < result.OldPosition))
                {
                    result = block;
                }
            }

            return result;
        }

        private MatcherBlock? GetBlock(long lastPosition, long oldStartPosition, long newStartPosition)
        {
            var oldPosition = Math.Max(0, oldStartPosition - _blockSize);
            var newPosition = Math.Max(0, newStartPosition - _blockSize);

            if (newPosition < lastPosition)
            {
                var shiftCount = lastPosition - newPosition;
                oldPosition += shiftCount;
                newPosition += shiftCount;
            }

            _oldStream.Position = oldPosition;
            _newStream.Position = newPosition;

            if (ReadBlock(oldStartPosition, oldPosition, out var blockCount, out var extraCount))
            {
                return new MatcherBlock(false, extraCount + blockCount, newStartPosition - extraCount, oldStartPosition - extraCount);
            }

            return null;
        }

        private bool ReadBlock(long oldStartPosition, long oldPosition, out int blockCount, out int extraCount)
        {
            blockCount = 0;
            extraCount = 0;

            while (true)
            {
                var oldByte = _oldStream.ReadByte();
                var newByte = _newStream.ReadByte();
                if (oldByte == -1 || newByte == -1) break;

                if (oldPosition >= oldStartPosition)
                {
                    if (oldByte != newByte) break;
                    blockCount++;
                }
                else if (oldByte == newByte)
                {
                    extraCount++;
                }
                else
                {
                    extraCount = 0;
                }

                oldPosition++;
            }

            return blockCount >= _blockSize;
        }

        #endregion

        #region Implementation of IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of IEnumerable<MatcherBlock>

        public IEnumerator<MatcherBlock> GetEnumerator()
        {
            var hashCount = 0L;
            var lastPosition = 0L;
            var readPosition = 0L;

            while (true)
            {
                // Read a byte and roll the hash.
                var newByte = _newStream.ReadByte();
                if (newByte == -1) break;
                _rollingHash.Add((byte)newByte);

                // Update the matcher state.
                hashCount++;
                readPosition++;

                // Find positions for the current hash.
                if (hashCount < _blockSize) continue;
                var oldStartPositions = _hashTable.Find(_rollingHash.Digest());
                if (oldStartPositions == null) continue;

                // Find the best block for the positions.
                var block = GetBestBlock(lastPosition, oldStartPositions, readPosition - _blockSize);
                if (block != null)
                {
                    // Find the copy block between two blocks.
                    var betweenCount = block.NewPosition - lastPosition;
                    if (betweenCount > 0) yield return new MatcherBlock(true, betweenCount, lastPosition);
                    yield return block;

                    // Update the matcher state.
                    lastPosition = block.NewPosition + block.Length;
                    readPosition = lastPosition;
                    hashCount = 0;
                }

                // Reset the stream position.
                _newStream.Position = readPosition;
            }

            // Find the copy block after the last block.
            var endCount = readPosition - lastPosition;
            if (endCount > 0) yield return new MatcherBlock(true, endCount, lastPosition);
        }

        #endregion
    }
}