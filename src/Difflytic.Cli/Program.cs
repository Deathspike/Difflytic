using System;
using System.IO;
using System.Reflection;
using Difflytic.Diffing;
using Difflytic.Hashing;
using Difflytic.Patching;

namespace Difflytic.Cli
{
    public static class Program
    {
        private const uint MaximumHashTableSize = 6553600;
        private const uint MinimumBlockSize = 16;

        #region Statics

        public static void Main(string[] args)
        {
            var options = Options.Create(args);
            var ver = GetVersion();
            Console.WriteLine(@" ____  _ ___ ___ _     _   _     ");
            Console.WriteLine(@"|    \|_|  _|  _| |_ _| |_|_|___ ");
            Console.WriteLine(@"|  |  | |  _|  _| | | |  _| |  _|");
            Console.WriteLine(@"|____/|_|_| |_| |_|_  |_| |_|___|");
            Console.WriteLine($"                  |___|    v{ver}");

            if (options.Data.Count > 2)
            {
                var diffPath = options.Data[^1];
                var newPaths = options.Data.Slice(1, options.Data.Count - 2);
                var oldPath = options.Data[0];
                var blockSize = GetBlockSize(options.BlockSize, oldPath);
                new Differ(blockSize, HashType.Adler32).Diff(diffPath, newPaths, oldPath);
            }
            else if (options.Data.Count == 2)
            {
                var oldPath = options.Data[0];
                var diffPath = options.Data[1];
                var outputPath = Path.GetDirectoryName(diffPath) ?? Environment.CurrentDirectory;
                Patcher.Patch(diffPath, oldPath, outputPath);
            }
        }

        private static int GetBlockSize(int blockSize, string oldPath)
        {
            if (blockSize > 0) return blockSize;
            return (int)Math.Max(MinimumBlockSize, new FileInfo(oldPath).Length / MaximumHashTableSize);
        }

        private static string GetVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            if (version == null) return "0.0.0";
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }

        #endregion
    }
}