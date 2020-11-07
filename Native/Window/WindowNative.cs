using System;
using System.Runtime.InteropServices;

namespace Memenim.Native.Window
{
    internal static class WindowNative
    {
        public const int HwndBroadcast = 0xFFFF;
        public const int WpfAsyncWindowPlacement = 2;
        public const int SwShowNormal = 1;
        public const int SwShowMinimized = 2;
        public const int SwShowMaximized = 3;

        public delegate bool EnumChildrenCallback(IntPtr hwnd, IntPtr lParam);

        // user32

        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hwnd, uint msg, IntPtr wparam, IntPtr lparam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern uint RegisterWindowMessage(string message);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPlacement(IntPtr hWnd, ref WindowPlacement lpwndpl);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowPlacement(IntPtr hWnd, ref WindowPlacement lpwndpl);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindow(IntPtr hWnd, int uCmd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetActiveWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool EnumThreadWindows(int dwThreadId, EnumChildrenCallback lpEnumFunc, IntPtr lParam);

        // kernel32

        [DllImport("kernel32.dll")]
        public static extern int GetCurrentThreadId();
    }
}
