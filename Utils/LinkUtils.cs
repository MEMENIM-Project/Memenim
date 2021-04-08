using System;
using System.Diagnostics;
using RIS;

namespace Memenim.Utils
{
    public static class LinkUtils
    {
        public static void OpenLink(string link)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = link,
                UseShellExecute = true
            };

            try
            {
                Process.Start(startInfo);
            }
            catch (Exception)
            {
                var exception = new Exception(
                    $"An error occurred when opening the link '{link}'");
                Events.OnError(new RErrorEventArgs(exception,
                    exception.Message));
            }
        }
    }
}
