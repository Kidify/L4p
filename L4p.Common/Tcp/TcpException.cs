using System;
using L4p.Common.Extensions;

namespace L4p.Common.Tcp
{
    public class TcpException : L4pException
    {
        public TcpException(string msg, Exception cause)
            : base(msg, cause)
        { }

        public TcpException(string msg, params object[] args)
            : base(msg, args)
        {}

        public TcpException(Exception cause, string msg, params object[] args)
            : base(cause, msg, args)
        {}
    }
}