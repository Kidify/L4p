using System;
using L4p.Common.Extensions;

namespace L4p.Common.Concerns
{
    public class ConcernsException : L4pException
    {
        public ConcernsException(string msg, Exception cause)
            : base(msg, cause)
        { }

        public ConcernsException(string msg, params object[] args)
            : base(msg, args)
        {}

        public ConcernsException(Exception cause, string msg, params object[] args)
            : base(cause, msg, args)
        {}
    }
}