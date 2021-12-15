﻿using System;
using System.Windows;

namespace Memenim.Native.Window.Utils
{
    internal static class WindowUtils
    {
        public static IntPtr HwndSourceHook(
            IntPtr hwnd, int msg,
            IntPtr wParam, IntPtr lParam,
            ref bool handled)
        {
            App.Instance.OnWndProc(
                Application.Current.MainWindow,
                hwnd, (uint)msg, wParam, lParam,
                true, true);

            return IntPtr.Zero;
        }



        public static void ActivateWindow(
            IntPtr hwnd)
        {
            ModalWindowUtils
                .ActivateWindow(hwnd);
        }

        public static IntPtr GetModalWindow(
            IntPtr owner)
        {
            return ModalWindowUtils
                .GetModalWindow(owner);
        }
    }
}
