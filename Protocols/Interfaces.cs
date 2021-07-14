using System;
using Memenim.Protocols.Schemas;

namespace Memenim.Protocols
{
    public interface IUserProtocol
    {
        string Name { get; }
        IUserProtocolSchema Schema { get; }

        bool Register();

        bool Exists();
    }
}
