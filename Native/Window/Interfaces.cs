using System;

namespace Memenim.Native.Window
{
    interface INativeRestorable
    {
        public bool DuringRestoreToMaximized { get; }
    }
}
