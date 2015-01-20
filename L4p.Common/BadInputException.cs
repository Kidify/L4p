using System;
using L4p.Common.Extensions;

namespace L4p.Common
{
    public class BadInputException : L4pException
    {
        public BadInputException()
            : base()
        { }

        public BadInputException(string msg)
            : base(msg)
        { }

        public BadInputException(string msg, Exception cause)
            : base(msg, cause)
        { }

        public BadInputException(string msg, params object[] args)
            : base(msg.Fmt(args))
        { }

        public BadInputException(Exception cause, string msg, params object[] args)
            : base(msg.Fmt(args), cause)
        { }
    }
}