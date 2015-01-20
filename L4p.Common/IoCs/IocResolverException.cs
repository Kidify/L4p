using System;

namespace L4p.Common.IoCs
{
    public class IocResolverException : L4pException
    {
        public IocResolverException(string msg, Exception cause)
            : base(msg, cause)
        { }

        public IocResolverException(string msg, params object[] args)
            : base(msg, args)
        {}

        public IocResolverException(Exception cause, string msg, params object[] args)
            : base(cause, msg, args)
        {}
    }
}