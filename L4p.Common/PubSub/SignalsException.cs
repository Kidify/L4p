using System;

namespace L4p.Common.PubSub
{
    [Serializable]
    public class SignalsException : L4pException
    {
        public SignalsException(string msg, Exception cause)
            : base(msg, cause)
        { }

        public SignalsException(string msg, params object[] args)
            : base(msg, args)
        { }

        public SignalsException(Exception cause, string msg, params object[] args)
            : base(cause, msg, args)
        { }
    }
}