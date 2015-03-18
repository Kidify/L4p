using System;
using System.Collections.Generic;
using System.Dynamic;
using L4p.Common.DumpToLogs;
using L4p.Common.Helpers;
using L4p.Common.Json;

namespace L4p.Common.MsgIsApplications
{
    public interface IMsgContext : IHaveDump
    {
        T Add<T>(T value) where T : class, new();
        T Get<T>() where T : class, new();
        T Remove<T>() where T : class, new();
        T Replace<T>(T value) where T : class, new();

        IMsgContext Impl { get; }
        IMsgContext Lock { set; }
        void SetMsgIsTheApp(MsgIsTheAppEx msgIsTheApp);       // friend access only
    }

    public class MsgContext : IMsgContext
    {
        #region counters

        class Counters
        {
            public int ValuesAdded;
            public int ValuesRemoved;
            public int VauesReplaced;
            public int ValuesRetrieved;
            public int ValueIsNotHere;
            public int MsgIsTheAppChanged;
            public int SameValueIsAdded;
        }

        #endregion

        #region members

        private readonly Counters _counters;
        private readonly Dictionary<Type, MsgValueBox> _values;

        private IMsgContext _lock;
        private IMsgIsTheApp _msgIsTheApp;

        #endregion

        #region construction

        public static IMsgContext New()
        {
            return
                new MsgContext();
        }

        public static IMsgContext NewSync()
        {
            var context = 
                SyncMsgContext.New(
                new MsgContext());

            context.Lock = context;

            return context;
        }

        private MsgContext()
        {
            _counters = new Counters();
            _values = new Dictionary<Type, MsgValueBox>();
            _msgIsTheApp = Null.MsgIsTheApp;
        }

        #endregion

        #region private

        private object get_value(Type type)
        {
            MsgValueBox box;
            _values.TryGetValue(type, out box);

            if (box == null)
                return null;

            return box.Value;
        }

        private void set_value(Type key, object value)
        {
            var box = new MsgValueBox {
                Type = key,
                Value = value
            };

            _values.Add(key, box);
        }

        #endregion

        #region interface

        ExpandoObject IHaveDump.Dump(dynamic root)
        {
            if (root == null)
                root = new ExpandoObject();

            root.Counters = _counters;
            root.Values = _values.Count;

            return root;
        }

        T IMsgContext.Add<T>(T value)
        {
            Validate.NotNull(value);

            var type = value.GetType();
            var prev = get_value(type);

            if (ReferenceEquals(prev, value))
            {
                _counters.SameValueIsAdded++;
                return value;
            }

            if (prev != null)
                throw new MsgIsTheAppException("Value for type '{0}' is already set; {1}", type.Name, prev.ToJson());

            _msgIsTheApp.SetContextFor(this, _lock, value);
            set_value(type, value);

            _counters.ValuesAdded++;

            return value;
        }

        T IMsgContext.Get<T>()
        {
            var type = typeof(T);
            var value = get_value(type);

            if (value != null)
                _counters.ValuesRetrieved++;
            else
                _counters.ValueIsNotHere++;

            return (T) value;
        }

        T IMsgContext.Remove<T>()
        {
            var type = typeof(T);
            var value = get_value(type);

            if (_values.Remove(type))
                _counters.ValuesRemoved++;

            return (T) value;
        }

        T IMsgContext.Replace<T>(T value)
        {
            Validate.NotNull(value);

            var type = value.GetType();
            var prev = get_value(type);

            if (ReferenceEquals(value, prev))
                return value;

            set_value(type, value);

            _counters.VauesReplaced++;

            return value;
        }

        IMsgContext IMsgContext.Impl
        {
            get { return this; }
        }

        IMsgContext IMsgContext.Lock
        {
            set { _lock = value; }
        }

        void IMsgContext.SetMsgIsTheApp(MsgIsTheAppEx msgIsTheApp)
        {
            Validate.NotNull(msgIsTheApp);

            if (ReferenceEquals(msgIsTheApp, _msgIsTheApp))
                return;

            _counters.MsgIsTheAppChanged++;

            _msgIsTheApp = msgIsTheApp;
        }

        #endregion
    }

    class SyncMsgContext : IMsgContext
    {
        private readonly object _mutex = new object();
        private readonly IMsgContext _impl;

        public static IMsgContext New(IMsgContext impl) { return new SyncMsgContext(impl); }
        private SyncMsgContext(IMsgContext impl) { _impl = impl; }

        ExpandoObject IHaveDump.Dump(dynamic root) { return _impl.Dump(root); }
        IMsgContext IMsgContext.Impl {get { return _impl.Impl; }}
        IMsgContext IMsgContext.Lock { set { _impl.Lock = value; } }
        T IMsgContext.Add<T>(T value) { lock(_mutex) return _impl.Add(value); }
        T IMsgContext.Get<T>() { lock(_mutex) return _impl.Get<T>(); }
        T IMsgContext.Remove<T>() { lock (_mutex) return _impl.Remove<T>(); }
        T IMsgContext.Replace<T>(T value) { lock(_mutex) return _impl.Replace(value); }
        void IMsgContext.SetMsgIsTheApp(MsgIsTheAppEx msgIsTheApp) { _impl.SetMsgIsTheApp(msgIsTheApp); }
    }
}