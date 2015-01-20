using System;
using System.Collections.Generic;
using System.Linq;

namespace L4p.Common.PubSub.contexts
{
    interface ISessionContextRepo
    {
        object GetContext(string key, Type type);
        object SetContext(string key, Type type, object context);
        void Clear(string key);
    }

    class SessionContextRepo : ISessionContextRepo
    {
        #region members

        private readonly Dictionary<Tuple<string, Type>, object> _state;

        #endregion

        #region construction

        public static ISessionContextRepo New()
        {
            return
                new SessionContextRepo();
        }

        public static ISessionContextRepo NewSync()
        {
            return
                SyncSessionContextRepo.New(
                new SessionContextRepo());
        }

        private SessionContextRepo()
        {
            _state = new Dictionary<Tuple<string, Type>, object>();
        }

        #endregion

        #region private
        #endregion

        #region interface

        object ISessionContextRepo.GetContext(string key, Type type)
        {
            object context;

            var tuple = Tuple.Create(key, type);
            _state.TryGetValue(tuple, out context);

            return context;
        }

        object ISessionContextRepo.SetContext(string key, Type type, object context)
        {
            object prev;

            var tuple = Tuple.Create(key, type);

            if (_state.TryGetValue(tuple, out prev))
                return prev;

            _state.Add(tuple, context);

            return context;
        }

        void ISessionContextRepo.Clear(string key)
        {
            var query =
                from tuple in _state.Keys
                where tuple.Item1 == key
                select tuple;

            var itemsToRemove = query.ToArray();

            foreach (var item in itemsToRemove)
            {
                _state.Remove(item);
            }
        }

        #endregion
    }

    class SyncSessionContextRepo : ISessionContextRepo
    {
        private readonly object _mutex = new object();
        private readonly ISessionContextRepo _impl;

        public static ISessionContextRepo New(ISessionContextRepo impl) { return new SyncSessionContextRepo(impl); }
        private SyncSessionContextRepo(ISessionContextRepo impl) { _impl = impl; }

        object ISessionContextRepo.GetContext(string key, Type type) { lock(_mutex) return _impl.GetContext(key, type); }
        object ISessionContextRepo.SetContext(string key, Type type, object context) { lock(_mutex) return _impl.SetContext(key, type, context); }
        void ISessionContextRepo.Clear(string key) { lock(_mutex) _impl.Clear(key); }
    }

}