using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Memenim.Protocols.Schemas.Api;
using RIS.Logging;

namespace Memenim.Protocols.Schemas
{
    public sealed class MemenimSchema : IUserProtocolSchema
    {
        public const string StaticName = "memenim";



        private static Dictionary<uint, IUserProtocolSchemaApi> UserApis { get; }

        public static MemenimSchema Instance { get; }



        public string Name
        {
            get
            {
                return StaticName;
            }
        }



        static MemenimSchema()
        {
            UserApis = new Dictionary<uint, IUserProtocolSchemaApi>();

            foreach (var api in GetUserApis())
            {
                UserApis.Add(
                    api.Version, api);
            }

            Instance = new MemenimSchema();
        }

        private MemenimSchema()
        {

        }



        private static IUserProtocolSchemaApi[] GetUserApis()
        {
            try
            {
                var types = Assembly
                    .GetExecutingAssembly()
                    .GetTypes()
                    .Where(type => type.IsClass
                                   && typeof(IUserProtocolSchemaApi).IsAssignableFrom(type))
                    .ToArray();
                var apis = new List<IUserProtocolSchemaApi>(types.Length);

                foreach (var type in types)
                {
                    var api = Activator.CreateInstance(
                        type, true) as IUserProtocolSchemaApi;

                    if (api == null)
                        continue;
                    if (api.SchemaName != StaticName)
                        continue;

                    apis.Add(api);
                }

                return apis.ToArray();
            }
            catch (Exception ex)
            {
                LogManager.Default.Error(ex,
                    "User protocol schema api's get error");

                return Array.Empty<IUserProtocolSchemaApi>();
            }
        }



        public bool ParseUri(Uri uri)
        {
            try
            {
                if (uri == null
                    || !uri.IsAbsoluteUri
                    || uri.Scheme != Name
                    || uri.Host != "app")
                {
                    return false;
                }

                var uriString = uri.OriginalString;
                var pathStartIndex = Name.Length + 6;

                if (uriString.Length < pathStartIndex + 2)
                    return false;

                var version = 1U;
                var versionDivideIndex = uriString
                    .IndexOf('/', pathStartIndex + 1);

                if (versionDivideIndex != -1
                    && uriString[pathStartIndex + 1] == 'v'
                    && uriString.Length > pathStartIndex + 2)
                {
                    var versionString = uriString.Substring(
                        pathStartIndex + 2,
                        versionDivideIndex - pathStartIndex - 2);

                    if (uint.TryParse(versionString, out version))
                    {
                        uriString = uriString.Remove(
                            pathStartIndex,
                            versionString.Length + 2);
                        uri = new Uri(
                            uriString, UriKind.Absolute);
                    }
                }

                if (version == 0)
                    version = 1;

                if (!UserApis.TryGetValue(version, out var api))
                    return false;

                return api?
                    .ParseUri(uri) ?? false;
            }
            catch (Exception ex)
            {
                LogManager.Default.Error(ex,
                    $"User protocol schema[Name = {Name}] parse uri error");

                return false;
            }
        }
    }
}
