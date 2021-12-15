using System;
using System.Text;
using ProtoBuf;

namespace Memenim.Native
{
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



        public IpcBusMessage(string name,
            byte[] content, IpcBusContentType type,
            bool restoreWindow = true)
        {
            Name = name;
            Content = content;
            ContentType = type;
            RestoreWindow = restoreWindow;
        }



        public string GetStringUtf8()
        {
            if (ContentType == IpcBusContentType.StringUtf8)
                return Encoding.UTF8.GetString(Content);

            return null;
        }
    }
}
