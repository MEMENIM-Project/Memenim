using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Environment = RIS.Environment;

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



        public static WindowPlacement GetPlacement(
            IntPtr windowHandle, bool throwOnError)
        {
            var placement = new WindowPlacement();

            if (windowHandle == IntPtr.Zero)
                return placement;

            placement.Length = Marshal.SizeOf(
                typeof(WindowPlacement));

            if (WindowNative.GetWindowPlacement(
                    windowHandle, ref placement))
            {
                return placement;
            }

            if (throwOnError)
            {
                throw new Win32Exception(
                    Marshal.GetLastWin32Error());
            }

            return new WindowPlacement();
        }



        public void SetPlacement(
            IntPtr windowHandle)
        {
            WindowNative.SetWindowPlacement(
                windowHandle, ref this);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct CopyData : IDisposable
    {
        public int DwData { get; set; }
        public int LpDataSize { get; set; }
        public IntPtr LpData { get; set; }

        public string AsAnsiString
        {
            get
            {
                return Marshal.PtrToStringAnsi(
                    LpData, LpDataSize);
            }
        }
        public string AsUnicodeString
        {
            get
            {
                return Marshal.PtrToStringUni(
                    LpData);
            }
        }



        public static CopyData CreateForString(
            int dwData, string value,
            bool unicode = true)
        {
            var result = new CopyData
            {
                DwData = dwData,
                LpData = unicode
                    ? Marshal.StringToCoTaskMemUni(value)
                    : Marshal.StringToCoTaskMemAnsi(value),
                LpDataSize = unicode
                    ? (value.Length + 1) *
                      sizeof(char)
                    : (value.Length + 1) *
                      Marshal.SystemMaxDBCSCharSize
            };

            return result;
        }


        public static bool SendString(IntPtr hwnd,
            uint msg, int dwData, string value,
            bool unicode = true)
        {
            var data = CreateForString(
                dwData, value, unicode);
            var dataSize = Environment
                .GetSize<CopyData>();
            var dataPtr = Marshal
                .AllocCoTaskMem(dataSize);

            Marshal.StructureToPtr(
                data, dataPtr, false);

            var messageReceived = WindowNative
                .SendMessage(hwnd, msg,
                    IntPtr.Zero, dataPtr)
                .ToInt32() != 0;

            data.Dispose();

            Marshal.FreeCoTaskMem(
                dataPtr);

            return messageReceived;
        }



        public void Dispose()
        {
            if (LpData == IntPtr.Zero)
                return;

            Marshal.FreeCoTaskMem(
                LpData);

            LpData = IntPtr.Zero;
            LpDataSize = 0;
        }
    }
}
