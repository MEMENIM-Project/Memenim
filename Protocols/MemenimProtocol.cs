﻿using System;
using Memenim.Protocols.Schemas;
using Microsoft.Win32;
using RIS.Logging;
using Environment = RIS.Environment;

namespace Memenim.Protocols
{
    public class MemenimProtocol : IUserProtocol
    {
        public string Name { get; }
        public IUserProtocolSchema Schema { get; }



        private MemenimProtocol()
        {
            Name = "MEMENIM App Protocol";
            Schema = new MemenimSchema();
        }



        public bool Register()
        {
            return RegisterInternal();
        }
        private bool RegisterInternal()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Name)
                    || string.IsNullOrWhiteSpace(Schema.Name))
                {
                    return false;
                }

                using (var schemeKey =
                    Registry.CurrentUser.CreateSubKey($@"SOFTWARE\Classes\{Schema.Name}"))
                {
                    if (schemeKey == null)
                        return false;

                    string filePath = Environment.ExecProcessFilePath;

                    schemeKey.SetValue(string.Empty,
                        "URL:" + Name);
                    schemeKey.SetValue("URL Protocol",
                        string.Empty);

                    using (var defaultIconKey =
                        schemeKey.CreateSubKey("DefaultIcon"))
                    {
                        if (defaultIconKey == null)
                            return false;

                        defaultIconKey.SetValue(string.Empty, $"\"{filePath}\"");
                    }

                    using (var commandKey =
                        schemeKey.CreateSubKey(@"shell\open\command"))
                    {
                        if (commandKey == null)
                            return false;

                        commandKey.SetValue(string.Empty,
                            $"\"{filePath}\" \"startupUri:%1\"");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Default.Error(ex, "Protocol register error");

                return false;
            }
        }


        public bool Exists()
        {
            return ExistsInternal();
        }
        private bool ExistsInternal()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Name)
                    || string.IsNullOrWhiteSpace(Schema.Name))
                {
                    return false;
                }

                using (var schemeKey =
                    Registry.CurrentUser.OpenSubKey($@"SOFTWARE\Classes\{Schema.Name}"))
                {
                    if (schemeKey != null)
                        return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                LogManager.Default.Error(ex, "Protocol check exists error");

                return false;
            }
        }
    }
}
