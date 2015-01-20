using System;
using L4p.Common.Extensions;

namespace L4p.Common.ActionMsgs
{
    public class ActionMsgException : L4pException
    {
        public ActionMsgException(string msg, Exception cause)
            : base(msg, cause)
        { }

        public ActionMsgException(string msg, params object[] args)
            : base(msg, args)
        {}

        public ActionMsgException(Exception cause, string msg, params object[] args)
            : base(cause, msg, args)
        {}
    }
}