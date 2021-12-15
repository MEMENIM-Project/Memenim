using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Memenim.Dialogs;
using Memenim.Utils;
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

            DownloadsPath = Path.Combine(
                Environment.ExecProcessDirectoryName,
                "downloads");

            if (!Directory.Exists(DownloadsPath))
                Directory.CreateDirectory(DownloadsPath);
        }



        private static string GetRandomFileName(
            string extension)
        {
            var resultFileName =
                $"{Path.GetRandomFileName()}{extension}";

            while (File.Exists(Path.Combine(DownloadsPath, resultFileName)))
            {
                resultFileName =
                    $"{Path.GetRandomFileName()}{extension}";
            }

            return resultFileName;
        }

        private static string GetFileName(
            string fileName)
        {
            var resultFileName = fileName;
            var counter = 1;

            while (File.Exists(Path.Combine(DownloadsPath, resultFileName)))
            {
                resultFileName =
                    $"{Path.GetFileNameWithoutExtension(fileName)} ({counter}){Path.GetExtension(fileName)}";

                ++counter;
            }

            return resultFileName;
        }



        public static Task SaveFile(
            string url, bool useRandomFileName = false,
            bool overwrite = false)
        {
            return SaveFile(new Uri(url),
                useRandomFileName, overwrite);
        }
        public static async Task SaveFile(
            Uri uri, bool useRandomFileName = false,
            bool overwrite = false)
        {
            var fileName = useRandomFileName
                ? GetRandomFileName(
                    Path.GetExtension(uri.Segments[^1]))
                : !overwrite
                    ? GetFileName(uri.Segments[^1])
                    : uri.Segments[^1];

            var path = Path.Combine(
                DownloadsPath, fileName);

            HttpResponseMessage response;

            try
            {
                response = await Client.GetAsync(uri)
                    .ConfigureAwait(true);
            }
            catch (Exception)
            {
                var message = LocalizationUtils
                    .GetLocalized("FailedToDownloadFileMessage");

                await DialogManager.ShowErrorDialog(message)
                    .ConfigureAwait(true);

                return;
            }

            if (!response.IsSuccessStatusCode)
            {
                var message = LocalizationUtils
                    .GetLocalized("FailedToDownloadFileMessage");

                await DialogManager.ShowErrorDialog(message)
                    .ConfigureAwait(true);

                return;
            }

            var stream = await response.Content.ReadAsStreamAsync()
                .ConfigureAwait(true);

            await using var file = File.Create(
                path, 65536);

            await stream.CopyToAsync(
                file, 65536);
        }

        public static Task SaveFile(
            Stream stream)
        {
            return SaveFile(stream,
                GetRandomFileName(".file"), true);
        }
        public static async Task SaveFile(
            Stream stream, string fileNameOrExtension,
            bool useRandomFileName = false,
            bool overwrite = false)
        {
            var fileName = useRandomFileName
                ? GetRandomFileName(
                    Path.GetExtension(fileNameOrExtension))
                : !overwrite
                    ? GetFileName(fileNameOrExtension)
                    : fileNameOrExtension;

            var path = Path.Combine(
                DownloadsPath, fileName);

            await using var file = File.Create(
                path, 65536);

            await stream.CopyToAsync(
                file, 65536);
        }
    }
}
