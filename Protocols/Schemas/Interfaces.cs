using System;

namespace Memenim.Protocols.Schemas
{
    public interface IUserProtocolSchema
    {
        string Name { get; }

        bool ParseUri(Uri uri);
    }
}
