using System;

namespace Memenim.Native.Window
{
    [Flags]
    internal enum SendMessageTimeoutFlags : uint
    {
        SmtoNormal = 0x0,
        SmtoBlock = 0x1,
        SmtoAbortIfHung = 0x2,
        SmtoNoTimeoutIfNotHung = 0x8
    }
}
