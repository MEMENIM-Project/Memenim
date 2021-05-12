using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.FileProviders;

namespace Memenim.Resources
{
    public static class ResourceManager
    {
        private static readonly EmbeddedFileProvider ResourceProvider;

        static ResourceManager()
        {
            ResourceProvider = new EmbeddedFileProvider(
                Assembly.GetExecutingAssembly(),
                $"Memenim.Resources");
        }

        public static byte[] GetEmbeddedAsBytes(
            string filePath)
        {
            using (var stream = ResourceProvider
                .GetFileInfo(filePath)
                .CreateReadStream())
            {
                if (stream == null)
                    return null;

                using (var reader = new BinaryReader(stream))
                {
                    return reader
                        .ReadBytes((int)reader.BaseStream.Length);
                }
            }
        }

        public static string GetEmbeddedAsString(
            string filePath)
        {
            using (var stream = ResourceProvider
                .GetFileInfo(filePath)
                .CreateReadStream())
            {
                if (stream == null)
                    return null;

                using (var reader = new StreamReader(stream))
                {
                    return reader
                        .ReadToEnd();
                }
            }
        }
    }
}
