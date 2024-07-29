using System.IO;
using System.Threading.Tasks;
using Difflytic.Patching.Reading;

namespace Difflytic.Patching
{
    public static class Patcher
    {
        #region Statics

        public static void Patch(string diffPath, string oldPath, string outputPath)
        {
            var reader = Reader.Create(diffPath, oldPath);
            WriteFiles(diffPath, oldPath, outputPath, reader);
            MoveFiles(outputPath, reader);
        }

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