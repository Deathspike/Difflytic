namespace Difflytic.Matching
{
    public sealed record MatcherBlock(bool IsCopy, long Length, long NewPosition, long OldPosition = -1);
}