using System;
using System.Collections.Generic;
using L4p.Common.Extensions;
using L4p.Common.Agers;

namespace L4p.Common.Loggers
{
    public class ThrottledLog : ILogFile
    {
        #region item

        class Item
        {
            public DateTime At { get; set; }
            public string Msg { get; set; }
        }

        #endregion

        #region members

        private readonly ILogFile _peer;
        private TimeSpan _ttl;
        private HashSet<string> _set;
        private readonly IAger<Item> _ager;

        #endregion

        #region construction

        public static ILogFile New(TimeSpan ttl, ILogFile log)
        {
            return
                new ThrottledLog(ttl, log);
        }

        public static ILogFile NewSync(TimeSpan ttl, ILogFile log)
        {
            return
                SyncThrottledLog.New(
                new ThrottledLog(ttl, log));
        }

        private ThrottledLog(TimeSpan ttl, ILogFile log)
        {
            _peer = log;
            _ttl = ttl;
            _set = new HashSet<string>();
            _ager = Ager<Item>.New(item => item.At);
        }

        #endregion

        #region private

        private bool should_be_filtered(string msg)
        {
            if (_set.Contains(msg))
                return true;

            return false;
        }

        private void add_to_filter(DateTime now, string msg)
        {
            var item = new Item
                {
                    At = now,
                    Msg = msg
                };

            _set.Add(item.Msg);
            _ager.Add(item);
        }

        private void remove_expired_items(DateTime now)
        {
            do
            {
                var item = _ager.GetExpiredItem(now, _ttl);

                if (item == null)
                    break;

                _set.Remove(item.Msg);
            }
            while (true);
        }

        private void dispatch(Action writeToLog, string msg, params object[] args)
        {
            if (_ttl == TimeSpan.Zero)
            {
                writeToLog();
                return;
            }

            var now = DateTime.UtcNow;
            var fmsg = msg.Fmt(args);

            remove_expired_items(now);

            if (should_be_filtered(fmsg))
                return;

            writeToLog();

            add_to_filter(now, fmsg);
        }

        #endregion

        #region interface

        ILogFile ILogFile.Error(string msg, params object[] args)
        {
            dispatch(
                () => _peer.Error(msg, args), msg, args);

            return this;
        }

        ILogFile ILogFile.Error(Exception ex)
        {
            dispatch(
                () => _peer.Error(ex), ex.Message);

            return this;
        }

        ILogFile ILogFile.Error(Exception ex, string msg, params object[] args)
        {
            dispatch(
                () => _peer.Error(ex, msg, args), msg, args);

            return this;
        }

        ILogFile ILogFile.Warn(string msg, params object[] args)
        {
            dispatch(
                () => _peer.Warn(msg, args), msg, args);

            return this;
        }

        ILogFile ILogFile.Warn(Exception ex, string msg, params object[] args)
        {
            dispatch(
                () => _peer.Warn(ex, msg, args), msg, args);

            return this;
        }

        ILogFile ILogFile.Info(string msg, params object[] args)
        {
            dispatch(
                () => _peer.Info(msg, args), msg, args);

            return this;
        }

        ILogFile ILogFile.Trace(string msg, params object[] args)
        {
            if (_peer.TraceOn == false)
                return this;

            dispatch(
                () => _peer.Trace(msg, args), msg, args);

            return this;
        }

        ILogFile ILogFile.NewFile()
        {
            _ager.Clear();
            _set.Clear();
            _peer.NewFile();

            return this;
        }

        string ILogFile.Name
        {
            get { return _peer.Name; }
        }

        string ILogFile.Path
        {
            get { return _peer.Path; }
        }

        bool ILogFile.TraceOn
        {
            get { return _peer.TraceOn; }
            set { _peer.TraceOn = value; }
        }

        #endregion
    }

    class SyncThrottledLog : ILogFile
    {
        private readonly object _mutex = new object();
        private readonly ILogFile _impl;

        public static ILogFile New(ILogFile impl) { return new SyncThrottledLog(impl); }
        private SyncThrottledLog(ILogFile impl) { _impl = impl; }

        ILogFile ILogFile.Error(string msg, params object[] args) { lock (_mutex) _impl.Error(msg, args); return this; }
        ILogFile ILogFile.Error(Exception ex) { lock(_mutex) _impl.Error(ex); return this; }
        ILogFile ILogFile.Error(Exception ex, string msg, params object[] args) { lock(_mutex) _impl.Error(ex, msg, args); return this; }
        ILogFile ILogFile.Warn(string msg, params object[] args) { lock(_mutex) _impl.Warn(msg, args); return this; }
        ILogFile ILogFile.Warn(Exception ex, string msg, params object[] args) { lock(_mutex) _impl.Warn(ex, msg, args); return this; }
        ILogFile ILogFile.Info(string msg, params object[] args) { lock(_mutex) _impl.Info(msg, args); return this; }
        ILogFile ILogFile.Trace(string msg, params object[] args) { lock(_mutex) _impl.Trace(msg, args); return this; }
        ILogFile ILogFile.NewFile() { lock(_mutex) _impl.NewFile(); return this; }


        string ILogFile.Name
        {
            get { return _impl.Name; }
        }

        string ILogFile.Path
        {
            get { return _impl.Path; }
        }

        bool ILogFile.TraceOn
        {
            get { return _impl.TraceOn; }
            set { _impl.TraceOn = value; }
        }
    }
}