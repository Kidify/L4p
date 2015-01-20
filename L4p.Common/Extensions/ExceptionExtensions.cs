using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace L4p.Common.Extensions
{
    public static class ExceptionExtensions
    {
        #region private

        class ExceptionInfo
        {
            public string ExType { get; set; }
            public string Msg { get; set; }
            public string File { get; set; }
            public string Method { get; set; }
            public int Line { get; set; }
        }

        private static string get_method_short_name(string fullName)
        {
            int lastCharAt = fullName.Length - 1;
            int firstDotAt = fullName.LastIndexOf('.', lastCharAt);

            if (firstDotAt == -1)
                return fullName;

            if (firstDotAt == 0)
                return fullName;

            int secondDotAt = fullName.LastIndexOf('.', firstDotAt - 1);

            if (secondDotAt == -1)
                return fullName;

            string shortName = fullName.Substring(secondDotAt + 1);

            return shortName;
        }

        private static ExceptionInfo get_exception_info(Exception ex)
        {
            var info = new ExceptionInfo();

            info.ExType = ex.GetType().Name;
            info.Msg = ex.Message;

            var stack = new StackTrace(ex, true);

            if (stack.FrameCount == 0)
                return info;

            var frame = stack.GetFrame(0);

            info.File = Path.GetFileName(frame.GetFileName());
            info.Method = get_method_short_name(frame.GetMethod().Name);
            info.Line = frame.GetFileLineNumber();

            return info;
        }

        private static Exception make_exception(Type type, string msg, Exception cause)
        {
            Exception exception;

            try
            {
                var ctor = type.GetConstructor(new[] { typeof(string), typeof(Exception) });
                exception = (Exception) ctor.Invoke(new object[] {msg, cause});
            }
            catch
            {
                msg += "; ({0})".Fmt(type.Name);
                exception = new L4pException(msg, cause);
            }

            return exception;
        }

        private static void set_exception_data(Exception exception, string key, object data)
        {
            try
            {
                exception.Data[key] = data;
            }
            catch (Exception ex)
            {
                exception.Data[key] = "Failed to add data of type '{0}'; {1}".Fmt(data.GetType().Name, ex.Message);
            }
        }

        private static string serialize_to_json(object data)
        {
            if (data == null)
                return "null";

            var dataAsStr = data as String;

            if (dataAsStr != null)
                return dataAsStr;

            try
            {
                dataAsStr = JsonConvert.SerializeObject(data, new StringEnumConverter());
            }
            catch (Exception ex)
            {
                dataAsStr = "Failed to serialize type '{0}'; {1}".Fmt(data.GetType().Name, ex.Message);
            }

            return dataAsStr;
        }

        private static Exception get_first_cause_exception(Exception ex)
        {
            var firstCause = ex;

            while (true)
            {
                var inner = firstCause.InnerException;

                if (inner == null)
                    break;

                firstCause = inner;
            }

            return firstCause;
        }

        private static void format_exception_data(string key, object data, StringBuilder sb)
        {
            const string indent = "       ";

            sb.StartNewLine();

            if (data == null)
            {
                sb
                    .AppendFormat("{0} '{1}': null", indent, key);

                return;
            }

            string itemAsStr = data as String;

            if (itemAsStr != null)
            {
                sb
                    .AppendFormat("{0} -- '{1}': {2}", indent, key, itemAsStr);

                return;
            }

            itemAsStr = serialize_to_json(data);

            sb
                .AppendFormat("{0} -- '{1}': {2}", indent, key, itemAsStr);
        }

        private static void format_exception_lines(Exception exception, StringBuilder sb, bool verbose)
        {
            if (exception == null)
                return;

            format_exception_lines(exception.InnerException, sb, verbose);

            var info = get_exception_info(exception);

            sb
                .StartNewLine()
                .AppendFormat("  ----> {0}  -- {1}", info.Msg, info.ExType);

            bool hasSourceInfo =
                info.File != null && info.Method != null;

            if (hasSourceInfo)
            {
                if (verbose)
                    sb.AppendFormat(" at {0}() {1}:{2}", info.Method, info.File, info.Line);
                else
                    sb.AppendFormat(" at {0}()", info.Method);
            }

            foreach (DictionaryEntry entry in exception.Data)
            {
                 string key = entry.Key.ToString();
                 object data = entry.Value;

                format_exception_data(key, data, sb);
            }
        }

        private static void format_exception_hierarchy(Exception exception, string msg, StringBuilder sb, bool verbose)
        {
            if (msg == null)
            {
                var firstCause = get_first_cause_exception(exception);

                sb
                    .StartNewLine()
                    .AppendFormat("{0} : {1}", firstCause.GetType().Name, firstCause.Message);
            }
            else
            {
                sb
                    .StartNewLine()
                    .Append(msg);
            }


            format_exception_lines(exception, sb, verbose);
        }

        private static void format_single_call_stack(Exception exception, StringBuilder sb)
        {
            sb
                .StartNewLine()
                .AppendFormat("--{0}", exception.GetType().FullName);

            var stack = new StackTrace(exception, true);
            var frames = stack.GetFrames();

            if (frames == null)
                return;

            foreach (var frame in frames)
            {
                string method = get_method_short_name(frame.GetMethod().Name);
                int line = frame.GetFileLineNumber();
                string file = Path.GetFileName(frame.GetFileName());

                sb
                    .StartNewLine()
                    .AppendFormat("  at {0}() {1}:{2}", method, file, line);
            }
        }

        private static void format_call_stack(Exception exception, StringBuilder sb)
        {
            if (exception == null)
                return;

            format_call_stack(exception.InnerException, sb);
            format_single_call_stack(exception, sb);
        }

        private static string format_hierarchy(Exception exception, string msg, bool withCallStack, bool verbose)
        {
            try
            {
                var sb = new StringBuilder();
                format_exception_hierarchy(exception, msg, sb, verbose);

                if (withCallStack)
                    format_call_stack(exception, sb);

                return
                    sb.ToString();
            }
            catch (Exception cause)
            {
                Debug.WriteLine(cause);

                return
                    exception.ToString();
            }
        }

        #endregion

        #region api

        public static Exception WrapWith<E>(this Exception cause, string msg, params object[] args)
            where E : Exception
        {
            Type type = typeof(E);
            string errMsg = msg.Fmt(args);

            return
                make_exception(type, errMsg, cause);
        }

        public static Exception SetData(this Exception self, string key, object data)
        {
            var dataAsStr = serialize_to_json(data);
            set_exception_data(self, key, dataAsStr);

            return self;
        }

        public static Exception SetData(this Exception self, object data)
        {
            return
                self.SetData(self.GetType().Name, data);
        }

        public static string FormatHierarchy(this Exception exception, bool withCallStack = true, bool verbose = false)
        {
            return
                format_hierarchy(exception, null, withCallStack, verbose);
        }

        public static string FormatHierarchy(this Exception exception, string msg, bool withCallStack = true, bool verbose = false)
        {
            return
                format_hierarchy(exception, msg, withCallStack, verbose);
        }

        #endregion
    }
}