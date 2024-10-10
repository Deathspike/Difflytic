using System.IO;
using System.Threading.Tasks;
using Difflytic.Patching.Reading;
using Microsoft.Win32.SafeHandles;

namespace Difflytic.Patching
{
    public static class Patcher
    {
        #region Statics

        public static void Patch(string diffPath, string oldPath, string outputPath)
        {
            using var diffHandle = File.OpenHandle(diffPath);
            using var oldHandle = File.OpenHandle(oldPath);
            Patch(diffHandle, oldHandle, outputPath);
        }

        public static void Patch(SafeFileHandle diffHandle, SafeFileHandle oldHandle, string outputPath)
        {
            var reader = Reader.Create(diffHandle, oldHandle);
            WriteFiles(outputPath, reader);
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

        private static void WriteFiles(string outputPath, Reader reader)
        {
            Parallel.ForEach(reader, file =>
            {
                using var inputStream = reader.Open(file);
                using var outputStream = File.OpenWrite(Path.Combine(outputPath, file.Name + ".diff.tmp"));
                outputStream.SetLength(0);
                inputStream.CopyTo(outputStream);
            });
        }

        #endregion
    }
}