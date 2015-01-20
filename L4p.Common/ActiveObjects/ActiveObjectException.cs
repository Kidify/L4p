using System;
using L4p.Common.Extensions;

namespace L4p.Common.ActiveObjects
{
    public class ActiveObjectException : L4pException
    {
        public ActiveObjectException(string msg, Exception cause)
            : base(msg, cause)
        { }

        public ActiveObjectException(string msg, params object[] args)
            : base(msg, args)
        {}

        public ActiveObjectException(Exception cause, string msg, params object[] args)
            : base(cause, msg, args)
        {}
    }
}