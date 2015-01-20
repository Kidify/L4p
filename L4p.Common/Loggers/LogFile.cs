using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using L4p.Common.Extensions;
using L4p.Common.Helpers;
using L4p.Common.NUnits;

namespace L4p.Common.Loggers
{
    public interface ILogFile
    {
        void Error(string msg, params object[] args);
        void Error(Exception ex);
        void Error(Exception ex, string msg, params object[] args);
        void Warn(string msg, params object[] args);
        void Warn(Exception ex, string msg, params object[] args);
        void Info(string msg, params object[] args);
        void Trace(string msg, params object[] args);

        string Name { get; }
        bool TraceOn { get; set; }
    }

    public class LogFile : ILogFile
    {
        #region members

        private const string LOG_FOLDER = "log_folder";
        public static string DEFAULT_LOG_FOLDER = @"d:\logs";

        private readonly string _name;
        private readonly string _path;
        private readonly bool _andTrace;
        private readonly string _processName;
        private readonly int _processId;

        private bool _traceOn;
        private bool _pemanentlyFailed;
        
        #endregion

        #region construction

        public static ILogFile New(string name, bool? andTrace = null)
        {
            bool andTrace_ = andTrace.GetValueOrDefault();

            if (andTrace == null)
                andTrace_ = false;

            return
                new LogFile(name, andTrace_);
        }

        private LogFile(string name, bool andTrace)
        {
            string logPath = get_log_path();
            var process = Process.GetCurrentProcess();

            _name = name;
            _path = Path.Combine(logPath, name);
            _andTrace = andTrace;
            _processName = process.ProcessName;
            _processId = process.Id;
            _traceOn = true;

            Try.Catch.Handle(
                () => Directory.CreateDirectory(logPath),
                ex => TraceLogger.WriteLine("Failed to create log path '{0}'; {1}".Fmt(_path, ex.Message)));
        }

        #endregion

        #region null

        private static readonly ILogFile _null = new NullLogFile();
        private static readonly ILogFile _console = StdLog.New();

        public static ILogFile Null { get { return _null; } }
        public static ILogFile Console { get { return _console; } }

        #endregion

        #region private

        private string get_log_path()
        {
            string logPath = ConfigurationManager.AppSettings[LOG_FOLDER];

            if (logPath == null)
            {
                logPath = DEFAULT_LOG_FOLDER;

                if (!NUnitHelpers.RunningUnderUnitTest)
                    TraceLogger.WriteLine("AppSettings['{0}'] does not exist; using default='{1}'".Fmt(LOG_FOLDER, logPath));
            }

            return logPath;
        }

        private void append_msg_to_file(string path, string msg)
        {
            using (TextWriter writer = File.AppendText(path))
            {
                writer.WriteLine(msg);
            }
        }

        private void retry(int count, Action action)
        {
            TimeSpan nextRetry = 10.Milliseconds();

            do
            {
                try
                {
                    action();
                    break;
                }
                catch
                {
                    nextRetry = nextRetry + nextRetry;
                    Thread.Sleep(nextRetry);
                }

                if (count-- == 0)
                {
                    _pemanentlyFailed = true;
                    break;
                }
            }
            while (true);
        }

        private string build_message(string priority, string msg)
        {
            var now = DateTime.UtcNow;
            var dateTime = now.ToString("dd-MM-yy HH:mm:ss");

            return
                "{0} {1} [{2}.{3}] {4}".Fmt(dateTime, priority, _processName, _processId, msg);
        }

        private void write_msg(string priority, string msg, params object[] args)
        {
            var formattedMsg = build_message(priority, msg.Fmt(args));

            if (_andTrace || _pemanentlyFailed)
                TraceLogger.WriteLine(formattedMsg);

            if (_pemanentlyFailed)
                return;

            retry(5, () => append_msg_to_file(_path, formattedMsg));
        }

        #endregion

        #region interface

        void ILogFile.Error(string msg, params object[] args)
        {
            write_msg("E", msg, args);
        }

        void ILogFile.Error(Exception ex)
        {
            write_msg("E", ex.FormatHierarchy());
        }

        void ILogFile.Error(Exception ex, string msg, params object[] args)
        {
            var sb = new StringBuilder();

            sb
                .Append(msg.Fmt(args))
                .StartNewLine()
                .Append(ex.FormatHierarchy());

            write_msg("E", sb.ToString());
        }

        void ILogFile.Warn(string msg, params object[] args)
        {
            write_msg("W", msg, args);
        }

        void ILogFile.Warn(Exception ex, string msg, params object[] args)
        {
            var sb = new StringBuilder();

            sb
                .Append(msg.Fmt(args))
                .StartNewLine()
                .Append(ex.FormatHierarchy());

            write_msg("W", sb.ToString());
        }

        void ILogFile.Info(string msg, params object[] args)
        {
            write_msg("I", msg, args);
        }

        void ILogFile.Trace(string msg, params object[] args)
        {
            if (_traceOn == false)
                return;

            write_msg("T", msg, args);
        }

        string ILogFile.Name
        {
            get { return _name; }
        }

        bool ILogFile.TraceOn
        {
            get { return _traceOn; }
            set { _traceOn = value; }
        }

        #endregion
    }
}