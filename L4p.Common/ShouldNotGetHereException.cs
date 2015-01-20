using System;

namespace L4p.Common
{
    public class ShouldNotGetHereException : L4pException
    {
        public ShouldNotGetHereException()
        { }

        public ShouldNotGetHereException(string msg, Exception cause)
            : base(msg, cause)
        { }

        public ShouldNotGetHereException(string msg, params object[] args)
            : base(msg, args)
        { }

        public ShouldNotGetHereException(Exception cause, string msg, params object[] args)
            : base(cause, msg, args)
        { }
    }
}