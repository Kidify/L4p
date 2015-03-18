using System;

namespace L4p.Common.MsgIsApplications
{
    public class MsgIsTheAppException : L4pException
    {
        public MsgIsTheAppException(string msg, Exception cause)
            : base(msg, cause)
        { }

        public MsgIsTheAppException(string msg, params object[] args)
            : base(msg, args)
        {}

        public MsgIsTheAppException(Exception cause, string msg, params object[] args)
            : base(cause, msg, args)
        {}
    }
}