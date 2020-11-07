using System;
using System.Threading;

namespace Memenim.Native.Window
{
    internal sealed class SingleInstanceApp
    {
        public Mutex Mutex { get; }
        public uint Message { get; }

        public SingleInstanceApp(string uniqueName)
        {
            if (uniqueName == null)
                throw new ArgumentNullException(nameof(uniqueName));

            Mutex = new Mutex(true, uniqueName);
            Message = WindowNative.RegisterWindowMessage("WM_" + uniqueName);
        }

        public void Run(Action action)
        {
            RunInternal(action, IntPtr.Zero, IntPtr.Zero);
        }

        private void RunInternal(Action action, IntPtr wParam, IntPtr lParam)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (!WaitMutex(wParam, lParam))
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

        public void OnWndProc(System.Windows.Window window, IntPtr hwnd, uint m,
            IntPtr wParam, IntPtr lParam, bool restorePlacement, bool activate)
        {
            if (!(window is INativeRestorableWindow restorableWindow))
            {
                OnWndProc(hwnd, m, wParam, lParam, restorePlacement, activate);

                return;
            }

            if (m != Message)
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
        public void OnWndProc(IntPtr hwnd, uint m, IntPtr wParam, IntPtr lParam,
           bool restorePlacement, bool activate)
        {
            if (m != Message)
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

        public bool WaitMutex(IntPtr wParam, IntPtr lParam)
        {
            return WaitMutex(false, wParam, lParam);
        }
        public bool WaitMutex(bool force, IntPtr wParam, IntPtr lParam)
        {
            bool exist = !WaitMutexInternal(force);

            if (exist)
            {
                _ = WindowNative.PostMessage((IntPtr) WindowNative.HwndBroadcast,
                    Message, wParam, lParam);
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
