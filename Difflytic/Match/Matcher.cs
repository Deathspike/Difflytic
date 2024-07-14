using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Difflytic.Hash;

namespace Difflytic.Match
{
    public sealed class Matcher : IEnumerable<MatcherResult>
    {
        private readonly int _blockSize;
        private readonly HashTable _hashTable;
        private readonly Stream _newStream;
        private readonly Stream _oldStream;
        private readonly IRollingHash _rollingHash;
        private long _copyPosition;

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

        public MatcherResult? GetBestBlock(IEnumerable<long> oldStartPositions, long newStartPosition)
        {
            MatcherResult? result = null;

            foreach (var oldStartPosition in oldStartPositions)
            {
                var block = GetBlock(oldStartPosition, newStartPosition);

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

        private MatcherResult? GetBlock(long oldStartPosition, long newStartPosition)
        {
            var oldPosition = Math.Max(0, oldStartPosition - _blockSize);
            var newPosition = Math.Max(0, newStartPosition - _blockSize);

            if (newPosition < _copyPosition)
            {
                var shiftCount = _copyPosition - newPosition;
                oldPosition += shiftCount;
                newPosition += shiftCount;
            }

            _oldStream.Position = oldPosition;
            _newStream.Position = newPosition;

            return ReadBlock(oldStartPosition, oldPosition, out var backCount, out var blockCount)
                ? new MatcherResult(false, backCount + blockCount, newStartPosition - backCount, oldStartPosition - backCount)
                : null;
        }

        private bool ReadBlock(long oldStartPosition, long oldPosition, out int backCount, out int blockCount)
        {
            backCount = 0;
            blockCount = 0;

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
                    backCount++;
                }
                else
                {
                    backCount = 0;
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

        #region Implementation of IEnumerable<MatcherResult>

        public IEnumerator<MatcherResult> GetEnumerator()
        {
            var hashCount = 0L;
            var readPosition = 0L;

            while (true)
            {
                var newByte = _newStream.ReadByte();
                if (newByte == -1) break;
                _rollingHash.Add((byte)newByte);

                hashCount++;
                readPosition++;

                if (hashCount < _blockSize) continue;
                var oldStartPositions = _hashTable.Find(_rollingHash.Digest());
                if (oldStartPositions == null) continue;

                var block = GetBestBlock(oldStartPositions, readPosition - _blockSize);
                if (block != null)
                {
                    var betweenCount = block.NewPosition - _copyPosition;
                    if (betweenCount > 0) yield return new MatcherResult(true, betweenCount, _copyPosition, -1);
                    yield return block;

                    hashCount = 0;
                    readPosition = block.NewPosition + block.Length;
                    _copyPosition = readPosition;
                }

                _newStream.Position = readPosition;
            }

            var endCount = readPosition - _copyPosition;
            if (endCount > 0) yield return new MatcherResult(true, endCount, _copyPosition, -1);
        }

        #endregion
    }
}