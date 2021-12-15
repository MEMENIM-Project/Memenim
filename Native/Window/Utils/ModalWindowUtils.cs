using System;

namespace Memenim.Native.Window.Utils
{
    internal static class ModalWindowUtils
    {
        public static bool IsOwned(
            IntPtr owner, IntPtr hwnd,
            ref int level)
        {
            while (true)
            {
                var ownerWindow = WindowNative
                    .GetWindow(hwnd, ModalWindow.GwOwner);

                if (ownerWindow == IntPtr.Zero)
                    return false;

                if (ownerWindow == owner)
                    return true;

                ++level;

                hwnd = ownerWindow;
            }
        }



        public static void ActivateWindow(
            IntPtr hwnd)
        {
            if (hwnd != IntPtr.Zero)
                WindowNative.SetActiveWindow(hwnd);
        }

        public static IntPtr GetModalWindow(
            IntPtr owner)
        {
            var window = new ModalWindow();

            WindowNative.EnumThreadWindows(
                WindowNative.GetCurrentThreadId(),
                window.EnumChildren, owner);

            return window.MaxOwnershipHandle;
        }
    }
}
