using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Memenim.Logs;

namespace Memenim.Protocols
{
    public static class ProtocolManager
    {
        private static Dictionary<string, IUserProtocol> UserProtocols { get; }

        static ProtocolManager()
        {
            UserProtocols = new Dictionary<string, IUserProtocol>();

            foreach (var protocol in GetUserProtocols())
            {
                UserProtocols.Add(protocol.SchemaName, protocol);
            }
        }

        private static IUserProtocol[] GetUserProtocols()
        {
            try
            {
                var types = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(type => type.IsClass && typeof(IUserProtocol).IsAssignableFrom(type))
                    .ToArray();
                var protocols = new List<IUserProtocol>(types.Length);

                foreach (var type in types)
                {
                    var protocol =
                        Activator.CreateInstance(type, true) as IUserProtocol;

                    if (protocol == null)
                        continue;

                    protocols.Add(protocol);
                }

                return protocols.ToArray();
            }
            catch (Exception ex)
            {
                LogManager.Log.Error(ex, "User protocols get error");
                return Array.Empty<IUserProtocol>();
            }
        }

        public static void RegisterAll()
        {
            foreach (var protocol in UserProtocols.Values)
            {
                protocol.Register();
            }
        }

        public static void ParseUri(string uriString)
        {
            if (string.IsNullOrEmpty(uriString))
                return;

            if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri)
                || !UserProtocols.ContainsKey(uri.Scheme))
            {
                return;
            }

            UserProtocols[uri.Scheme].Schema?.ParseUri(uriString);
        }
    }
}
