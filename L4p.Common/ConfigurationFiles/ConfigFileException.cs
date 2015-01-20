using System;

namespace L4p.Common.ConfigurationFiles
{
    public class ConfigFileException : L4pException
    {
        public ConfigFileException(string msg, Exception cause)
            : base(msg, cause)
        { }

        public ConfigFileException(string msg, params object[] args)
            : base(msg, args)
        { }

        public ConfigFileException(Exception cause, string msg, params object[] args)
            : base(cause, msg, args)
        { }
    }
}