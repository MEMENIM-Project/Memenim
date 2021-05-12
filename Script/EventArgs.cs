using System;
using Memenim.Script.Entities;

namespace Memenim.Script
{
    public class ScriptInformationEventArgs : EventArgs
    {
        public Exception SourceException { get; }
        public MemenimScriptModule Module { get; }
        private string _message;
        public string Message
        {
            get
            {
                return _message
                       ?? SourceException?.Message
                       ?? string.Empty;
            }
            private set
            {
                _message = value;
            }
        }

        public ScriptInformationEventArgs(
            MemenimScriptModule module, string message)
            : this(null, module, message)
        {

        }
        public ScriptInformationEventArgs(Exception sourceException,
            MemenimScriptModule module, string message = null)
        {
            SourceException = sourceException;
            Module = module;
            Message = message;
        }
    }

    public class ScriptWarningEventArgs : EventArgs
    {
        public Exception SourceException { get; }
        public MemenimScriptModule Module { get; }
        private string _message;
        public string Message
        {
            get
            {
                return _message
                       ?? SourceException?.Message
                       ?? string.Empty;
            }
            private set
            {
                _message = value;
            }
        }

        public ScriptWarningEventArgs(
            MemenimScriptModule module, string message)
            : this(null, module, message)
        {

        }
        public ScriptWarningEventArgs(Exception sourceException,
            MemenimScriptModule module, string message = null)
        {
            SourceException = sourceException;
            Module = module;
            Message = message;
        }
    }

    public class ScriptErrorEventArgs : EventArgs
    {
        public Exception SourceException { get; }
        public MemenimScriptModule Module { get; }
        private string _message;
        public string Message
        {
            get
            {
                return _message
                       ?? SourceException?.Message
                       ?? string.Empty;
            }
            private set
            {
                _message = value;
            }
        }

        public ScriptErrorEventArgs(
            MemenimScriptModule module, string message)
            : this(null, module, message)
        {

        }
        public ScriptErrorEventArgs(Exception sourceException,
            MemenimScriptModule module, string message = null)
        {
            SourceException = sourceException;
            Module = module;
            Message = message;
        }
    }

    public class ScriptLoadedEventArgs : EventArgs
    {
        public MemenimScriptModule Module { get; }

        public ScriptLoadedEventArgs(MemenimScriptModule module)
        {
            Module = module;
        }
    }

    public class ScriptUnloadedEventArgs : EventArgs
    {
        public Exception SourceException { get; }
        public MemenimScriptModule Module { get; }

        public ScriptUnloadedEventArgs(
            MemenimScriptModule module)
            : this(null, module)
        {

        }
        public ScriptUnloadedEventArgs(Exception sourceException,
            MemenimScriptModule module)
        {
            SourceException = sourceException;
            Module = module;
        }
    }
}
