namespace Difflytic.Hashing.MLCG
{
    public static class MLCG
    {
        public static readonly uint[] Powers;

        #region Constructors

        static MLCG()
        {
            Powers = new uint[256];
            Powers[255] = 1;
            for (var i = 254; i >= 0; i--) Powers[i] = Powers[i + 1] * 851723965;
        }

        #endregion
    }
}