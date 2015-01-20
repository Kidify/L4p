using System;

namespace L4p.Common.FunnelsModel
{
    class FunnelsException : L4pException
    {
        public FunnelsException(string msg, Exception cause)
            : base(msg, cause)
        { }

        public FunnelsException(string msg, params object[] args)
            : base(msg, args)
        {}

        public FunnelsException(Exception cause, string msg, params object[] args)
            : base(cause, msg, args)
        {}
    }
}