using System;
using System.Globalization;
using System.IO;
using Memenim.Resources;
using NLog.Config;
using RIS;

namespace Memenim.Logging
{
    public static class LogManager
    {
        private static readonly object LogSyncRoot = new object();
        private static volatile NLog.Logger _log;
        public static NLog.Logger Log
        {
            get
            {
                if (_log == null)
                {
                    lock (LogSyncRoot)
                    {
                        if (_log == null)
                            _log = NLog.LogManager.GetLogger(nameof(Log));
                    }
                }

                return _log;
            }
        }
        private static readonly object DebugLogSyncRoot = new object();
        private static volatile NLog.Logger _debugLog;
        public static NLog.Logger DebugLog
        {
            get
            {
                if (_debugLog == null)
                {
                    lock (DebugLogSyncRoot)
                    {
                        if (_debugLog == null)
                            _debugLog = NLog.LogManager.GetLogger(nameof(DebugLog));
                    }
                }

                return _debugLog;
            }
        }

        static LogManager()
        {
            NLog.LogManager.Configuration = XmlLoggingConfiguration
                .CreateFromXmlString(ResourceManager
                    .GetEmbeddedAsString(@"Configs\nlog.config"));
            NLog.LogManager.AutoShutdown = true;
            NLog.LogManager.Flush();

            DebugLog.Info("Logger initialized");
            Log.Info("Logger initialized");
        }

        public static int DeleteLogs(string filesDirectoryPath,
            int retentionDaysPeriod)
        {
            return DeleteLogsInternal(filesDirectoryPath, retentionDaysPeriod, "log");
        }
        private static int DeleteLogsInternal(string filesDirectoryPath,
            int retentionDaysPeriod, string fileExtension)
        {
            if (retentionDaysPeriod < 0)
                return 0;

            filesDirectoryPath = filesDirectoryPath
                .TrimEnd(Path.DirectorySeparatorChar)
                .TrimEnd(Path.AltDirectorySeparatorChar);

            filesDirectoryPath = !Path.IsPathRooted(filesDirectoryPath)
                ? Path.GetFullPath(filesDirectoryPath)
                : filesDirectoryPath;

            fileExtension = fileExtension.StartsWith('.')
                ? fileExtension
                : $".{fileExtension}";

            if (!Directory.Exists(filesDirectoryPath))
            {
                var exception = new DirectoryNotFoundException($"Cannot start log files deletion. Directory '{filesDirectoryPath}' not found");
                Events.OnError(new RErrorEventArgs(exception, exception.Message));
                throw exception;
            }

            try
            {
                int countDeletedFiles = 0;
                string currentFileNameDate = NLog.GlobalDiagnosticsContext.Get("AppStartupTime");
                DateTime nowDate = DateTime.UtcNow;
                string[] logFiles = Directory.GetFiles(filesDirectoryPath, $"*{fileExtension}");

                for (int i = 0; i < logFiles.Length; ++i)
                {
                    ref string logFile = ref logFiles[i];

                    if (logFile == null)
                        continue;

                    try
                    {
                        if (Path.GetFileNameWithoutExtension(logFile)?.StartsWith(currentFileNameDate) != false)
                            continue;

                        DateTime logFileDate = DateTime.ParseExact(
                            Path.GetFileNameWithoutExtension(logFile)?.Substring(0, 19) ?? string.Empty,
                            "yyyy.MM.dd HH-mm-ss", CultureInfo.CurrentCulture).ToUniversalTime();

                        if (retentionDaysPeriod != 0
                            && logFileDate.AddDays(retentionDaysPeriod) > nowDate)
                        {
                            continue;
                        }

                        File.Delete(logFile);
                        ++countDeletedFiles;
                    }
                    catch (Exception ex)
                    {
                        Events.OnError(new RErrorEventArgs(ex, ex.Message));
                    }
                }

                return countDeletedFiles;
            }
            catch (Exception ex)
            {
                Events.OnError(new RErrorEventArgs(ex, ex.Message));
                throw;
            }
        }
    }
}
