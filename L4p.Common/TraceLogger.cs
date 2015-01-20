using System;
using System.Diagnostics;
using System.Text;
using L4p.Common.Extensions;
using L4p.Common.NUnits;

namespace L4p.Common
{
    public static class TraceLogger
    {
        static TraceLogger()
        {
            if (NUnitHelpers.RunningUnderUnitTest == false)
                Trace.Listeners.Add(new ConsoleTraceListener());
        }

        public static void WriteLine(string fmt, params object[] args)
        {
            var msg = fmt.Fmt(args);
            Trace.WriteLine(msg);
        }

        public static void WriteLine(Exception ex)
        {
            Trace.WriteLine(ex.FormatHierarchy(true, true));
        }

        public static void WriteLine(Exception ex, string fmt, params object[] args)
        {
            var msg = fmt.Fmt(args);
            Trace.WriteLine(ex.FormatHierarchy(msg, true, true));
        }

        public static void WriteLine(StringBuilder sb)
        {
            Trace.WriteLine(sb.ToString());
        }
    }
}