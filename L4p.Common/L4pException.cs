using System;
using L4p.Common.Extensions;

namespace L4p.Common
{
    public class L4pException : Exception
    {
        public L4pException()
            : base()
        {}

        public L4pException(string msg)
            : base(msg)
        {}

        public L4pException(string msg, Exception cause)
            : base(msg, cause)
        {}

        public L4pException(string msg, params object[] args)
            : base(msg.Fmt(args))
        {}

        public L4pException(Exception cause, string msg, params object[] args)
            : base(msg.Fmt(args), cause)
        {}
    }
}