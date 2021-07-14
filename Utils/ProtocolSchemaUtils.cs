using System;
using System.Reflection;
using RIS.Logging;
using RIS.Reflection.Mapping;

namespace Memenim.Utils
{
    public static class ProtocolSchemaUtils
    {
        public static void LogMethodError(Exception exception,
            MethodBase method, string args)
        {
            if (method == null)
            {
                LogManager.Log.Error(exception, $"Schema mapped method with args [{args}] error");

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

            LogManager.Log.Error(exception, $"Schema {methodType} [{methodName}] with args [{args}] error");
        }
    }
}
