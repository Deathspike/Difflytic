using System.Collections.Generic;

namespace Difflytic.Cli
{
    public sealed class Options
    {
        #region Constructors

        private Options()
        {
            Data = new List<string>();
        }

        public static Options Create(string[] args)
        {
            var options = new Options();
            options.Init(args);
            return options;
        }

        #endregion

        #region Methods

        private void Init(IReadOnlyList<string> args)
        {
            for (var i = 0; i < args.Count; i++)
            {
                switch (args[i])
                {
                    case "--block-size":
                        if (i + 1 >= args.Count || !int.TryParse(args[i + 1], out var blockSize)) break;
                        BlockSize = blockSize;
                        i++;
                        break;
                    default:
                        Data.Add(args[i]);
                        break;
                }
            }
        }

        #endregion

        #region Properties

        public int BlockSize { get; private set; }
        public List<string> Data { get; }

        #endregion
    }
}