using System;
using L4p.Common;

namespace L4p.WebApi
{
    public class WebApiException : L4pException
    {
        public WebApiException(string msg, Exception cause) : base(msg, cause)                                      {}
        public WebApiException(string msg, params object[] args) : base(msg, args)                                  {}
        public WebApiException(Exception cause, string msg, params object[] args) : base(cause, msg, args)          {}
    }

    public class BadRequestException : WebApiException
    {
        public BadRequestException(string msg, Exception cause) : base(msg, cause) { }
        public BadRequestException(string msg, params object[] args) : base(msg, args) { }
        public BadRequestException(Exception cause, string msg, params object[] args) : base(cause, msg, args) { }
    }

    public class UriIsNotFoundException : BadRequestException
    {
        public UriIsNotFoundException(string msg, Exception cause) : base(msg, cause)                               {}
        public UriIsNotFoundException(string msg, params object[] args) : base(msg, args)                           {}
        public UriIsNotFoundException(Exception cause, string msg, params object[] args) : base(cause, msg, args)   {}
    }
}