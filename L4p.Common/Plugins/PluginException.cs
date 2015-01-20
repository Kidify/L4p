using System;

namespace L4p.Common.Plugins
{
    public class PluginException : L4pException
    {
        public PluginException(string msg, Exception cause)
            : base(msg, cause)
        { }

        public PluginException(string msg, params object[] args)
            : base(msg, args)
        { }

        public PluginException(Exception cause, string msg, params object[] args)
            : base(cause, msg, args)
        { }
    }
}