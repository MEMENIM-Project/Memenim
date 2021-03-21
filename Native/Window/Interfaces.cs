using System;

namespace Memenim.Native.Window
{
    interface INativeRestorableWindow
    {
        public bool DuringRestoreToMaximized { get; }
    }
}
