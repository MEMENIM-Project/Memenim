using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Memenim.Native.Window;
using Memenim.Native.Window.Utils;
using Memenim.Protocols;
using ProtoBuf;
using RIS.Extensions;
using RIS.Logging;
using TinyIpc.Messaging;

namespace Memenim.Native
{
    internal sealed class SingleInstanceApp
    {
        private TinyMessageBus InstanceBus { get; set; }
        private TinyMessageBus LocalBus { get; set; }



        public string UniqueName { get; }
        public Mutex Mutex { get; }
        public uint SingleInstanceMessage { get; }



        public SingleInstanceApp(
            string uniqueName)
        {
            if (string.IsNullOrWhiteSpace(uniqueName))
                throw new ArgumentNullException(nameof(uniqueName));

            LocalBus = new TinyMessageBus(
                $"Bus_{uniqueName}");

            UniqueName = uniqueName;
            Mutex = new Mutex(
                true, uniqueName);
            SingleInstanceMessage = WindowNative
                .RegisterWindowMessage($"WM_{uniqueName}");
        }



        private static byte[] WrapMessage(
            string name, byte[] content,
            IpcBusContentType type,
            bool restoreWindow)
        {
            if (content == null || content.Length == 0)
                return Array.Empty<byte>();

            using var memoryStream =
                new MemoryStream(content.Length);
            var message = new IpcBusMessage(
                name, content, type,
                restoreWindow);

            Serializer.Serialize(
                memoryStream, message);

            return memoryStream
                .ToArray();
        }

        private static IpcBusMessage? UnwrapMessage(
            byte[] messageBytes)
        {
            if (messageBytes == null || messageBytes.Length == 0)
                return null;

            using var memoryStream =
                new MemoryStream(messageBytes);
            var message = Serializer
                .Deserialize<IpcBusMessage>(
                    memoryStream);

            return message;
        }



        public Task Run(
            Action runAction)
        {
            return RunInternal(runAction,
                IntPtr.Zero, IntPtr.Zero);
        }
        private async Task RunInternal(
            Action runAction,
            IntPtr wParam, IntPtr lParam)
        {
            if (runAction == null)
                throw new ArgumentNullException(nameof(runAction));

            var firstInstance = await WaitMutex(
                    wParam, lParam)
                .ConfigureAwait(true);

            if (!firstInstance)
                return;

            try
            {
                runAction();
            }
            finally
            {
                ReleaseMutex();
            }
        }



        public Task SendMessageStringUtf8(
            string name, string content,
            bool restoreWindow = true)
        {
            return SendMessageInternal(name,
                Encoding.UTF8.GetBytes(content),
                IpcBusContentType.StringUtf8,
                restoreWindow);
        }

        private Task SendMessageInternal(
            string name, byte[] content,
            IpcBusContentType type,
            bool restoreWindow = true)
        {
            var message = WrapMessage(
                name, content, type,
                restoreWindow);

            if (message == null || message.Length == 0)
                return Task.CompletedTask;

            LogManager.Debug.Info($"Send message - name = {name}, " +
                                 $"type = {type}, restoreWindow = {restoreWindow}");

            if (type == IpcBusContentType.StringUtf8)
            {
                LogManager.Debug.Info($"Send message content - " +
                                      $"{Encoding.UTF8.GetString(content)}");
            }

            return LocalBus.PublishAsync(
                message);
        }



        public void OnWndProc(
            System.Windows.Window window,
            IntPtr hwnd, uint msg,
            IntPtr wParam, IntPtr lParam,
            bool restorePlacement, bool activate)
        {
            if (msg != SingleInstanceMessage)
                return;

            if (restorePlacement)
            {
                var placement = WindowPlacement
                    .GetPlacement(hwnd, false);

                if (placement.IsValid && placement.IsMinimized)
                {
                    placement.Flags |= WindowNative.WpfAsyncWindowPlacement;

                    placement.ShowCmd =
                        window is INativelyRestorableWindow { DuringRestoreToMaximized: true }
                            ? WindowNative.SwShowMaximized
                            : WindowNative.SwShowNormal;

                    placement.SetPlacement(
                        hwnd);
                }
            }

            if (!activate)
                return;

            WindowNative.SetForegroundWindow(
                hwnd);
            WindowUtils.ActivateWindow(
                WindowUtils.GetModalWindow(hwnd));
        }


        public static void ActivateWindow(
            IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
                return;

            WindowUtils.ActivateWindow(
                WindowUtils.GetModalWindow(hwnd));
        }



        public Task<bool> WaitMutex(
            IntPtr wParam, IntPtr lParam)
        {
            return WaitMutex(false,
                wParam, lParam);
        }
        public async Task<bool> WaitMutex(
            bool force, IntPtr wParam, IntPtr lParam)
        {
            var firstInstance = WaitMutexInternal(force);

            if (firstInstance)
            {
                InstanceBus = new TinyMessageBus($"Bus_{UniqueName}");
                InstanceBus.MessageReceived += InstanceBus_MessageReceived;
            }
            else
            {
                var originalArgs = Environment
                    .GetCommandLineArgs();
                var args =
                    Array.Empty<string>();

                if (originalArgs.Length > 0)
                    args = originalArgs.Skip(1).ToArray();

                if (args.Length > 0)
                {
                    await SendMessageStringUtf8(
                            "SendArgs",
                            string.Join(" || ", args))
                        .ConfigureAwait(true);
                }
                else
                {
                    _ = WindowNative.PostMessage(
                        (IntPtr)WindowNative.HwndBroadcast,
                        SingleInstanceMessage,
                        wParam, lParam);
                }
            }

            return firstInstance;
        }
        private bool WaitMutexInternal(
            bool force)
        {
            if (force)
                return true;

            try
            {
                return Mutex.WaitOne(
                    TimeSpan.Zero, true);
            }
            catch (AbandonedMutexException)
            {
                return true;
            }
        }

        public void ReleaseMutex()
        {
            Mutex.ReleaseMutex();
        }



        private void InstanceBus_MessageReceived(object sender,
            TinyMessageReceivedEventArgs e)
        {
            var message = UnwrapMessage(
                e.Message);

            if (!message.HasValue)
                return;

            LogManager.Debug.Info($"Receive message - name = {message.Value.Name}, " +
                                     $"type = {message.Value.ContentType}, restoreWindow = {message.Value.RestoreWindow}");

            if (message.Value.ContentType == IpcBusContentType.StringUtf8)
            {
                LogManager.Debug.Info($"Receive message content - " +
                                      $"{message.Value.GetStringUtf8()}");
            }

            if (message.Value.Name == "SendArgs")
            {
                var args = message.Value
                    .GetStringUtf8()
                    .Split(" || ");
                var wrapper = App.UnwrapArgs(
                    args);

                foreach (var (key, value) in wrapper.Enumerate())
                {
                    switch (key)
                    {
                        case "startupUri":
                            var startupUri = (string)value;

                            if (!string.IsNullOrEmpty(startupUri))
                                ProtocolManager.ParseUri(startupUri);

                            break;
                        case "createHashFiles":
                            var createHashFilesString = (string)value;
                            bool createHashFiles;

                            try
                            {
                                createHashFiles = string.IsNullOrWhiteSpace(createHashFilesString)
                                                  || createHashFilesString.ToBoolean();
                            }
                            catch (Exception)
                            {
                                createHashFiles = false;
                            }

                            if (createHashFiles)
                                App.CreateHashFiles();

                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                return;
            }

            if (message.Value.RestoreWindow)
            {
                _ = WindowNative.PostMessage(
                    (IntPtr)WindowNative.HwndBroadcast,
                    SingleInstanceMessage,
                    IntPtr.Zero, IntPtr.Zero);
            }
        }
    }
}
