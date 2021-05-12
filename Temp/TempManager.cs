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
            TempPath = Path.Combine(Environment.ExecProcessDirectoryName,
                "temp");

            if (!Directory.Exists(TempPath))
                Directory.CreateDirectory(TempPath);
        }

        private static string GetFileName(string fileName)
        {
            string resultFileName = fileName;
            string path = Path.Combine(TempPath, fileName);

            foreach (var file in Directory.GetFiles(TempPath))
            {
                if (file != path)
                    continue;

                int counter = 1;

                while (File.Exists(Path.Combine(TempPath, resultFileName)))
                {
                    resultFileName = $"{Path.GetFileNameWithoutExtension(fileName)} ({counter}){Path.GetExtension(fileName)}";
                    ++counter;
                }
            }

            return resultFileName;
        }

        public static async Task<string> SaveFile(string fileName,
            Stream stream, bool overwrite = false)
        {
            fileName = !overwrite
                ? GetFileName(fileName)
                : fileName;
            string path = Path.Combine(TempPath, fileName);

            using (FileStream file = File.Create(path, (int)stream.Length))
            {
                byte[] data = new byte[stream.Length];

                await stream.ReadAsync(data, 0, data.Length)
                    .ConfigureAwait(true);

                await file.WriteAsync(data, 0, data.Length)
                    .ConfigureAwait(true);
            }

            return path;
        }
    }
}
