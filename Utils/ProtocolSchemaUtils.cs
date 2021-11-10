using System;
using System.Collections.Specialized;
using System.Reflection;
using RIS.Logging;
using RIS.Reflection.Mapping;

namespace Memenim.Utils
{
    public static class ProtocolSchemaUtils
    {
        public static void LogApiMethodError(Exception exception,
            MethodBase method, string schemaName,
            uint apiVersion, NameValueCollection args)
        {
            var argsString = args
                .ToString()?
                .TrimStart('?');

            LogApiMethodError(exception,
                method, schemaName,
                apiVersion, argsString);
        }
        public static void LogApiMethodError(Exception exception,
            MethodBase method, string schemaName,
            uint apiVersion, string args)
        {
            if (method == null)
            {
                LogManager.Default.Error(exception,
                    $"User protocol schema api[SchemaName = {schemaName}, Version = {apiVersion}] mapped method invoked with args[{args}] error");

                return;
            }

            string methodType = null;
            string methodName = null;

            if (method.IsDefined(typeof(MappedMethodAttribute)))
            {
                methodType = "mapped method";
                methodName = method.GetCustomAttribute<MappedMethodAttribute>()?
                    .Name;
            }

            if (string.IsNullOrEmpty(methodName))
            {
                methodType = "method";
                methodName = method
                    .Name;
            }

            LogManager.Default.Error(exception,
                $"User protocol schema api[SchemaName = {schemaName}, Version = {apiVersion}] {methodType}[{methodName}] invoked with args[{args}] error");
        }
    }
}
