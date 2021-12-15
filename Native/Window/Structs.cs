using System;
using Memenim.Native.Window.Utils;

namespace Memenim.Native.Window
{
    internal struct ModalWindow
    {
        public const int GwOwner = 4;



        private int _maxOwnershipLevel;



        public IntPtr MaxOwnershipHandle { get; private set; }



        public bool EnumChildren(
            IntPtr hwnd, IntPtr lParam)
        {
            var level = 1;

            if (WindowNative.IsWindowVisible(hwnd)
                && ModalWindowUtils.IsOwned(lParam, hwnd, ref level)
                && level > _maxOwnershipLevel)
            {
                MaxOwnershipHandle = hwnd;
                _maxOwnershipLevel = level;
            }

            return true;
        }
    }
}
