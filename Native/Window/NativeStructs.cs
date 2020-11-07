using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Memenim.Native.Window
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowPlacement
    {
        public int Length { get; set; }
        public int Flags { get; set; }
        public int ShowCmd { get; set; }
        public int MinPositionX { get; set; }
        public int MinPositionY { get; set; }
        public int MaxPositionX { get; set; }
        public int MaxPositionY { get; set; }
        public int NormalPositionLeft { get; set; }
        public int NormalPositionTop { get; set; }
        public int NormalPositionRight { get; set; }
        public int NormalPositionBottom { get; set; }

        public bool IsMinimized
        {
            get
            {
                return ShowCmd == WindowNative.SwShowMinimized;
            }
        }
        public bool IsValid
        {
            get
            {
                return Length == Marshal.SizeOf(typeof(WindowPlacement));
            }
        }

        public static WindowPlacement GetPlacement(IntPtr windowHandle, bool throwOnError)
        {
            WindowPlacement placement = new WindowPlacement();

            if (windowHandle == IntPtr.Zero)
                return placement;

            placement.Length = Marshal.SizeOf(typeof(WindowPlacement));

            if (!WindowNative.GetWindowPlacement(windowHandle, ref placement))
            {
                if (throwOnError)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                return new WindowPlacement();
            }

            return placement;
        }

        public void SetPlacement(IntPtr windowHandle)
        {
            WindowNative.SetWindowPlacement(windowHandle, ref this);
        }
    }
}
