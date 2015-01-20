using System;

namespace L4p.Common.Loggers
{
    public class NullLogFile : ILogFile
    {
        void ILogFile.Error(string msg, params object[] args) {}
        void ILogFile.Error(Exception ex) { }
        void ILogFile.Error(Exception ex, string msg, params object[] args) {}
        void ILogFile.Warn(string msg, params object[] args) {}
        void ILogFile.Warn(Exception ex, string msg, params object[] args) { }
        void ILogFile.Info(string msg, params object[] args) {}
        void ILogFile.Trace(string msg, params object[] args) {}
        string ILogFile.Name {get { return String.Empty; } }
        bool ILogFile.TraceOn { get; set; }
    }

    public static class NullLogFileWrapper
    {
        public static ILogFile WrapIfNull(this ILogFile log)
        {
            return
                log ?? LogFile.Null;
        }
    }
}