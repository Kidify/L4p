using System;
using L4p.Common.Extensions;

namespace L4p.Common.ExcelFiles
{
    public class ExcelFileConfig
    {
        public TimeSpan MinimalCheckSpan { get; set; }
        public string OpCode { get; set; }
        public string OpTitles { get; set; }

        public ExcelFileConfig()
        {
            MinimalCheckSpan = 2.Seconds();
            OpCode = Const.OpCode;
            OpTitles = Const.Titles;
        }

        #region constants

        public class Const
        {
            public const string OpCode = "OpCode";
            public const string Titles = "Titles";
        }

        #endregion
    }
}