using System;
using System.Globalization;
using L4p.Common.IoCs;
using L4p.Common.Loggers;

namespace L4p.Common.PubSub.contexts
{
    public interface ISessionContext
    {
        TContext Get<TContext>(int key) where TContext : class;
        TContext Get<TContext>(Guid key) where TContext : class;
        TContext Get<TContext>(string key) where TContext : class;

        TContext Set<TContext>(int key, TContext context) where TContext : class;
        TContext Set<TContext>(Guid key, TContext context) where TContext : class;
        TContext Set<TContext>(string key, TContext context) where TContext : class;

        TContext GetOrCreate<TContext>(int key) where TContext : class, new();
        TContext GetOrCreate<TContext>(Guid key) where TContext : class, new();
        TContext GetOrCreate<TContext>(string key) where TContext : class, new();

        void Clear(int key);
        void Clear(Guid key);
        void Clear(string key);
    }

    class SessionContext : ISessionContext
    {
        #region members

        private readonly ILogFile _log;
        private readonly ISessionContextRepo _repo;

        #endregion

        #region construction

        public static ISessionContext New(IIoC ioc)
        {
            return
                new SessionContext(ioc);
        }

        private SessionContext(IIoC ioc)
        {
            _log = ioc.Resolve<ILogFile>();
            _repo = SessionContextRepo.NewSync();
        }

        #endregion

        #region private

        private T get_context<T>(string key)
            where T : class
        {
            var type = typeof(T);

            return
                (T) _repo.GetContext(key, type);
        }

        private T set_context<T>(string key, T context)
            where T : class
        {
            var type = typeof(T);

            return
                (T) _repo.SetContext(key, type, context);
        }

        private T get_or_create<T>(string key)
            where T : class, new()
        {
            var type = typeof(T);

            var context = _repo.GetContext(key, type);

            if (context == null)
            {
                context = new T();
                context = _repo.SetContext(key, type, context);
            }

            return (T) context;
        }

        private void clear(string key)
        {
            _repo.Clear(key);
        }

        #endregion

        #region interface

        TContext ISessionContext.Get<TContext>(int key)
        {
            return
                get_context<TContext>(key.ToString(CultureInfo.InvariantCulture));
        }

        TContext ISessionContext.Get<TContext>(Guid key)
        {
            return
                get_context<TContext>(key.ToString());
        }

        TContext ISessionContext.Get<TContext>(string key)
        {
            return
                get_context<TContext>(key);
        }

        TContext ISessionContext.Set<TContext>(int key, TContext context)
        {
            return
                set_context(key.ToString(CultureInfo.InvariantCulture), context);
        }

        TContext ISessionContext.Set<TContext>(Guid key, TContext context)
        {
            return
                set_context(key.ToString(), context);
        }

        TContext ISessionContext.Set<TContext>(string key, TContext context)
        {
            return
                set_context(key, context);
        }

        TContext ISessionContext.GetOrCreate<TContext>(int key)
        {
            return
                get_or_create<TContext>(key.ToString(CultureInfo.InvariantCulture));
        }

        TContext ISessionContext.GetOrCreate<TContext>(Guid key)
        {
            return
                get_or_create<TContext>(key.ToString());
        }

        TContext ISessionContext.GetOrCreate<TContext>(string key)
        {
            return
                get_or_create<TContext>(key);
        }

        void ISessionContext.Clear(int key)
        {
            clear(key.ToString(CultureInfo.InvariantCulture));
        }

        void ISessionContext.Clear(Guid key)
        {
            clear(key.ToString());
        }

        void ISessionContext.Clear(string key)
        {
            clear(key);
        }

        #endregion
    }
}