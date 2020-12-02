using System;
using System.Text;
using ProtoBuf;

namespace Memenim.Native.Window
{
    internal struct ModalWindow
    {
        public const int GwOwner = 4;

        private int _maxOwnershipLevel;

        public IntPtr MaxOwnershipHandle { get; private set; }

        public bool EnumChildren(IntPtr hwnd, IntPtr lParam)
        {
            int level = 1;

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

    [ProtoContract]
    [Serializable]
    internal struct IpcBusMessage
    {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public byte[] Content { get; set; }
        [ProtoMember(3)]
        public IpcBusContentType ContentType { get; set; }
        [ProtoMember(4)]
        public bool RestoreWindow { get; set; }

        public IpcBusMessage(string name, byte[] content,
            IpcBusContentType type, bool restoreWindow = true)
        {
            Name = name;
            Content = content;
            ContentType = type;
            RestoreWindow = restoreWindow;
        }

        public string GetStringUtf8()
        {
            if (ContentType != IpcBusContentType.StringUtf8)
                return null;

            return Encoding.UTF8.GetString(Content);
        }
    }
}
