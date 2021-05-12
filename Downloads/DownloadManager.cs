using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Environment = RIS.Environment;

namespace Memenim.Downloads
{
    public static class DownloadManager
    {
        private static readonly HttpClient Client;

        public static readonly string DownloadsPath;

        static DownloadManager()
        {
            Client = new HttpClient();
            DownloadsPath = Path.Combine(Environment.ExecProcessDirectoryName,
                "downloads");

            if (!Directory.Exists(DownloadsPath))
                Directory.CreateDirectory(DownloadsPath);
        }

        private static string GetFileName(string fileName)
        {
            string resultFileName = fileName;
            string path = Path.Combine(DownloadsPath, fileName);

            foreach (var file in Directory.GetFiles(DownloadsPath))
            {
                if (file != path)
                    continue;

                int counter = 1;

                while (File.Exists(Path.Combine(DownloadsPath, resultFileName)))
                {
                    resultFileName = $"{Path.GetFileNameWithoutExtension(fileName)} ({counter}){Path.GetExtension(fileName)}";
                    ++counter;
                }
            }

            return resultFileName;
        }

        public static Task SaveFile(string url)
        {
            return SaveFile(new Uri(url));
        }
        public static async Task SaveFile(Uri uri)
        {
            string fileName = GetFileName(uri.Segments[^1]);
            string path = Path.Combine(DownloadsPath, fileName);

            HttpResponseMessage response = await Client.GetAsync(uri)
                .ConfigureAwait(true);

            if (response.IsSuccessStatusCode)
            {
                Stream stream = await response.Content.ReadAsStreamAsync()
                    .ConfigureAwait(true);

                using (FileStream file = File.Create(path, (int)stream.Length))
                {
                    byte[] data = new byte[stream.Length];

                    await stream.ReadAsync(data, 0, data.Length)
                        .ConfigureAwait(true);

                    await file.WriteAsync(data, 0, data.Length)
                        .ConfigureAwait(true);
                }
            }
            else
            {
                throw new FileNotFoundException();
            }
        }
        public static async Task SaveFile(string fileName, Stream stream)
        {
            fileName = GetFileName(fileName);
            string path = Path.Combine(DownloadsPath, fileName);

            using (FileStream file = File.Create(path, (int)stream.Length))
            {
                byte[] data = new byte[stream.Length];

                await stream.ReadAsync(data, 0, data.Length)
                    .ConfigureAwait(true);

                await file.WriteAsync(data, 0, data.Length)
                    .ConfigureAwait(true);
            }
        }
    }
}
