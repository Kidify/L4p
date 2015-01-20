using System;
using L4p.Common.Extensions;

namespace L4p.Common.ForeverThreads
{
    public class ForeverThreadException : L4pException
    {
        public ForeverThreadException(string msg, Exception cause)
            : base(msg, cause)
        { }

        public ForeverThreadException(string msg, params object[] args)
            : base(msg, args)
        {}

        public ForeverThreadException(Exception cause, string msg, params object[] args)
            : base(cause, msg, args)
        {}
    }
}