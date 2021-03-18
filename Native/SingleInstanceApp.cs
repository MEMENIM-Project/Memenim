using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Memenim.Logging;
using Memenim.Native.Window;
using Memenim.Protocols;
using ProtoBuf;
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

        public SingleInstanceApp(string uniqueName)
        {
            if (uniqueName == null)
                throw new ArgumentNullException(nameof(uniqueName));

            UniqueName = uniqueName;

            LocalBus = new TinyMessageBus($"Bus_{uniqueName}");

            Mutex = new Mutex(true, uniqueName);
            SingleInstanceMessage = WindowNative.RegisterWindowMessage($"WM_{uniqueName}");
        }

        private void InstanceBus_MessageReceived(object sender, TinyMessageReceivedEventArgs e)
        {
            var message = UnwrapMessage(e.Message);

            if (!message.HasValue)
                return;

            LogManager.DebugLog.Info($"Receive message - name = {message.Value.Name}, " +
                                     $"type = {message.Value.ContentType}, restoreWindow = {message.Value.RestoreWindow}");

            if (message.Value.ContentType == IpcBusContentType.StringUtf8)
                LogManager.DebugLog.Info($"Receive message content - {message.Value.GetStringUtf8()}");

            if (message.Value.Name == "SendArgs")
            {
                string[] args = message.Value.GetStringUtf8().Split(" || ");
                var wrapper = App.UnwrapArgs(args);

                foreach (var argEntry in wrapper.Enumerate())
                {
                    switch (argEntry.Key)
                    {
                        case "startupUri":
                            string startupUri = (string)argEntry.Value;

                            if (!string.IsNullOrEmpty(startupUri))
                                ProtocolManager.ParseUri(startupUri);

                            break;
                        case "createHashFiles":
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
                _ = WindowNative.PostMessage((IntPtr)WindowNative.HwndBroadcast,
                    SingleInstanceMessage, IntPtr.Zero, IntPtr.Zero);
            }
        }

        private static byte[] WrapMessage(string name, byte[] content,
            IpcBusContentType type, bool restoreWindow)
        {
            if (content == null || content.Length == 0)
                return Array.Empty<byte>();

            using var memoryStream = new MemoryStream(content.Length);
            var message = new IpcBusMessage(name, content,
                type, restoreWindow);
            Serializer.Serialize(memoryStream, message);

            return memoryStream.ToArray();
        }

        private static IpcBusMessage? UnwrapMessage(byte[] messageBytes)
        {
            if (messageBytes == null || messageBytes.Length == 0)
                return null;

            using var memoryStream = new MemoryStream(messageBytes);
            var message = Serializer.Deserialize<IpcBusMessage>(memoryStream);

            return message;
        }

        private Task SendMessage(string name, byte[] content,
            IpcBusContentType type, bool restoreWindow = true)
        {
            byte[] message = WrapMessage(name, content,
                type, restoreWindow);

            if (message == null || message.Length == 0)
                return Task.CompletedTask;

            LogManager.DebugLog.Info($"Send message - name = {name}, " +
                                 $"type = {type}, restoreWindow = {restoreWindow}");

            if (type == IpcBusContentType.StringUtf8)
                LogManager.DebugLog.Info($"Send message content - {Encoding.UTF8.GetString(content)}");

            return LocalBus.PublishAsync(message);
        }

        public Task SendMessageStringUtf8(string name, string content,
            bool restoreWindow = true)
        {
            return SendMessage(name, Encoding.UTF8.GetBytes(content),
                IpcBusContentType.StringUtf8, restoreWindow);
        }

        public void Run(Action action)
        {
            RunInternal(action, IntPtr.Zero, IntPtr.Zero).Wait();
        }

        private async Task RunInternal(Action action, IntPtr wParam, IntPtr lParam)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            bool alreadyRunning = !(await WaitMutex(wParam, lParam)
                .ConfigureAwait(true));

            if (alreadyRunning)
                return;

            try
            {
                action();
            }
            finally
            {
                ReleaseMutex();
            }
        }

        public static void ActivateWindow(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
                return;

            WindowUtils.ActivateWindow(WindowUtils.GetModalWindow(hwnd));
        }

        public void OnWndProc(System.Windows.Window window, IntPtr hwnd, uint msg,
            IntPtr wParam, IntPtr lParam, bool restorePlacement, bool activate)
        {
            if (!(window is INativeRestorableWindow restorableWindow))
            {
                OnWndProc(hwnd, msg, wParam, lParam, restorePlacement, activate);

                return;
            }

            if (msg != SingleInstanceMessage)
                return;

            if (restorePlacement)
            {
                WindowPlacement placement = WindowPlacement.GetPlacement(hwnd, false);

                if (placement.IsValid && placement.IsMinimized)
                {
                    placement.Flags |= WindowNative.WpfAsyncWindowPlacement;

                    placement.ShowCmd = restorableWindow.DuringRestoreToMaximized
                        ? WindowNative.SwShowMaximized
                        : WindowNative.SwShowNormal;

                    placement.SetPlacement(hwnd);
                }
            }

            if (!activate)
                return;

            WindowNative.SetForegroundWindow(hwnd);
            WindowUtils.ActivateWindow(WindowUtils.GetModalWindow(hwnd));
        }
        public void OnWndProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam,
           bool restorePlacement, bool activate)
        {
            if (msg != SingleInstanceMessage)
                return;

            if (restorePlacement)
            {
                WindowPlacement placement = WindowPlacement.GetPlacement(hwnd, false);

                if (placement.IsValid && placement.IsMinimized)
                {
                    placement.ShowCmd = WindowNative.SwShowNormal;

                    placement.SetPlacement(hwnd);
                }
            }

            if (!activate)
                return;

            WindowNative.SetForegroundWindow(hwnd);
            WindowUtils.ActivateWindow(WindowUtils.GetModalWindow(hwnd));
        }

        public Task<bool> WaitMutex(IntPtr wParam, IntPtr lParam)
        {
            return WaitMutex(false, wParam, lParam);
        }
        public async Task<bool> WaitMutex(bool force, IntPtr wParam, IntPtr lParam)
        {
            bool exist = !WaitMutexInternal(force);

            if (exist)
            {
                string[] originalArgs = Environment.GetCommandLineArgs();
                string[] args = new string[originalArgs.Length - 1];

                if (args.Length > 0)
                    args = originalArgs.Skip(1).ToArray();

                if (args.Length > 0)
                {
                    await SendMessageStringUtf8("SendArgs",
                            string.Join(" || ", args))
                        .ConfigureAwait(true);
                }
                else
                {
                    _ = WindowNative.PostMessage((IntPtr)WindowNative.HwndBroadcast,
                        SingleInstanceMessage, wParam, lParam);
                }
            }
            else
            {
                InstanceBus = new TinyMessageBus($"Bus_{UniqueName}");
                InstanceBus.MessageReceived += InstanceBus_MessageReceived;
            }

            return !exist;
        }

        private bool WaitMutexInternal(bool force)
        {
            if (force)
                return true;

            try
            {
                return Mutex.WaitOne(TimeSpan.Zero, true);
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
    }
}
