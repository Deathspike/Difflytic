namespace Difflytic.Patching.Reading
{
    public sealed record ReaderFile(long DataLength, long HeaderCount, long HeaderLength, string Name)
    {
        #region Properties

        public long DataPosition { get; set; } = -1;
        public long HeaderPosition { get; set; } = -1;

        #endregion
    }
}