using System;
using System.Text;
using L4p.Common.Extensions;

namespace L4p.Common.Loggers
{
    public class StdLog : ILogFile
    {
        #region members

        private readonly string _name;
        private bool _traceOn;
        
        #endregion

        #region construction

        public static ILogFile New()
        {
            return
                new StdLog();
        }

        private StdLog()
        {
            _name = "StdLog";
            _traceOn = true;
        }

        #endregion

        #region private

        private string build_message(string priority, string msg)
        {
            return
                "{0} {1}".Fmt(priority, msg);
        }

        private void write_msg(string priority, string msg, params object[] args)
        {
            var formattedMsg = build_message(priority, msg.Fmt(args));
            TraceLogger.WriteLine(formattedMsg);
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