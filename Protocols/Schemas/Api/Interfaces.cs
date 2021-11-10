using System;

namespace Memenim.Protocols.Schemas.Api
{
    public interface IUserProtocolSchemaApi
    {
        uint Version { get; }
        string SchemaName { get; }

        bool ParseUri(Uri uri);
    }
}
