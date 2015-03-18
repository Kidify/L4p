using System;

namespace L4p.Common.Loggers
{
    public class NullLogFile : ILogFile
    {
        ILogFile ILogFile.Error(string msg, params object[] args) { return this; }
        ILogFile ILogFile.Error(Exception ex) { return this; }
        ILogFile ILogFile.Error(Exception ex, string msg, params object[] args) { return this; }
        ILogFile ILogFile.Warn(string msg, params object[] args) { return this; }
        ILogFile ILogFile.Warn(Exception ex, string msg, params object[] args) { return this; }
        ILogFile ILogFile.Info(string msg, params object[] args) { return this; }
        ILogFile ILogFile.Trace(string msg, params object[] args) { return this; }
        ILogFile ILogFile.NewFile() { return this; }
        string ILogFile.Name {get { return String.Empty; } }
        string ILogFile.Path { get { return String.Empty; } }
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