using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RIS.Logging;

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
                UserProtocols.Add(
                    protocol.Schema.Name, protocol);
            }
        }



        private static IUserProtocol[] GetUserProtocols()
        {
            try
            {
                var types = Assembly
                    .GetExecutingAssembly()
                    .GetTypes()
                    .Where(type => type.IsClass
                                   && typeof(IUserProtocol).IsAssignableFrom(type))
                    .ToArray();
                var protocols = new List<IUserProtocol>(types.Length);

                foreach (var type in types)
                {
                    var protocol = Activator.CreateInstance(
                        type, true) as IUserProtocol;

                    if (protocol == null)
                        continue;

                    protocols.Add(protocol);
                }

                return protocols.ToArray();
            }
            catch (Exception ex)
            {
                LogManager.Default.Error(ex,
                    "User protocols get error");

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

        public static bool ParseUri(string uriString)
        {
            try
            {
                if (string.IsNullOrEmpty(uriString))
                    return false;

                if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri)
                    || !UserProtocols.ContainsKey(uri.Scheme)
                    || uri.Host != "app")
                {
                    return false;
                }

                if (!UserProtocols.TryGetValue(uri.Scheme, out var protocol))
                    return false;

                return protocol?
                    .Schema?
                    .ParseUri(uri) ?? false;
            }
            catch (Exception ex)
            {
                LogManager.Default.Error(ex,
                    "User protocol parse uri error");

                return false;
            }
        }
    }
}
