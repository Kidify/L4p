using System;

namespace L4p.Common.ExcelFiles
{
    public class ExcelFileException : L4pException
    {
        public ExcelFileException(string msg, Exception cause)
            : base(msg, cause)
        { }

        public ExcelFileException(string msg, params object[] args)
            : base(msg, args)
        { }

        public ExcelFileException(Exception cause, string msg, params object[] args)
            : base(cause, msg, args)
        { }
    }
}