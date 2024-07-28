using System.IO;
using System.Threading.Tasks;
using Difflytic.Hashing;
using Difflytic.Patching.Reading;

namespace Difflytic.Patching
{
    public sealed class Patcher
    {
        private readonly IHashFactory _hashFactory;

        #region Constructors

        public Patcher(IHashFactory hashFactory)
        {
            _hashFactory = hashFactory;
        }

        #endregion

        #region Methods

        public void Patch(string diffPath, string oldPath, string outputPath)
        {
            var blockHash = _hashFactory.CreateBlockHash();
            var reader = Reader.Create(blockHash, diffPath, oldPath);
            WriteFiles(diffPath, oldPath, outputPath, reader);
            MoveFiles(outputPath, reader);
        }

        #endregion

        #region Statics

        private static void MoveFiles(string outputPath, Reader reader)
        {
            foreach (var file in reader)
            {
                var destinationPath = Path.Combine(outputPath, file.Name);
                var sourcePath = Path.Combine(outputPath, file.Name + ".diff.tmp");
                File.Move(sourcePath, destinationPath, true);
            }
        }

        private static void WriteFiles(string diffPath, string oldPath, string outputPath, Reader reader)
        {
            Parallel.ForEach(reader, file =>
            {
                using var inputStream = ReaderFileStream.Create(diffPath, file, oldPath);
                using var outputStream = new BufferedStream(File.OpenWrite(Path.Combine(outputPath, file.Name + ".diff.tmp")));
                outputStream.SetLength(0);
                inputStream.CopyTo(outputStream);
            });
        }

        #endregion
    }
}