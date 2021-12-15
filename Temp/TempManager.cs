using System;
using System.IO;
using System.Threading.Tasks;
using Environment = RIS.Environment;

namespace Memenim.Temp
{
    public static class TempManager
    {
        public static readonly string TempPath;



        static TempManager()
        {
            TempPath = Path.Combine(
                Environment.ExecProcessDirectoryName,
                "temp");

            if (!Directory.Exists(TempPath))
                Directory.CreateDirectory(TempPath);
        }



        private static string GetRandomFileName()
        {
            var resultFileName =
                $"{Path.GetRandomFileName()}.temp";

            while (File.Exists(Path.Combine(TempPath, resultFileName)))
            {
                resultFileName =
                    $"{Path.GetRandomFileName()}.temp";
            }

            return resultFileName;
        }

        private static string GetFileName(
            string fileName)
        {
            var resultFileName = fileName;
            var counter = 1;

            while (File.Exists(Path.Combine(TempPath, resultFileName)))
            {
                resultFileName =
                    $"{Path.GetFileNameWithoutExtension(fileName)} ({counter}){Path.GetExtension(fileName)}";

                ++counter;
            }

            return resultFileName;
        }



        public static Task<string> SaveFile(
            Stream stream)
        {
            return SaveFile(stream,
                GetRandomFileName(), true);
        }
        public static async Task<string> SaveFile(
            Stream stream, string fileName,
            bool overwrite = false)
        {
            fileName = !overwrite
                ? GetFileName(fileName)
                : fileName;

            var path = Path.Combine(
                TempPath, fileName);

            await using var file = File.Create(
                path, 65536);

            await stream.CopyToAsync(
                file, 65536);

            return path;
        }
    }
}
