using System;

namespace Memenim.Native.Window
{
    interface INativelyRestorableWindow
    {
        public bool DuringRestoreToMaximized { get; }
    }
}
