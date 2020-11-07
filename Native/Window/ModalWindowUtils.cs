using System;

namespace Memenim.Native.Window
{
    internal static class ModalWindowUtils
    {
        public static bool IsOwned(IntPtr owner, IntPtr hwnd, ref int level)
        {
            IntPtr ownerWindow = WindowNative.GetWindow(hwnd, ModalWindow.GwOwner);

            if (ownerWindow == IntPtr.Zero)
                return false;

            if (ownerWindow == owner)
                return true;

            level++;

            return IsOwned(owner, ownerWindow, ref level);
        }

        public static void ActivateWindow(IntPtr hwnd)
        {
            if (hwnd != IntPtr.Zero)
                WindowNative.SetActiveWindow(hwnd);
        }

        public static IntPtr GetModalWindow(IntPtr owner)
        {
            ModalWindow window = new ModalWindow();

            WindowNative.EnumThreadWindows(WindowNative.GetCurrentThreadId(),
                window.EnumChildren, owner);

            return window.MaxOwnershipHandle;
        }
    }
}
