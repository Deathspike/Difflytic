namespace Difflytic.Match
{
    public sealed class MatcherResult(bool isCopy, long length, long newPosition, long oldPosition)
    {
        #region Properties

        public bool IsCopy { get; } = isCopy;
        public long Length { get; } = length;
        public long NewPosition { get; } = newPosition;
        public long OldPosition { get; } = oldPosition;

        #endregion
    }
}