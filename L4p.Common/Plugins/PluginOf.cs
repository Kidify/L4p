using System;
using System.Diagnostics;

namespace L4p.Common.Plugins
{
    public abstract class PluginOf<TImpl>
        where TImpl : class
    {
        #region members

        private readonly Type _likeThis;
        private readonly IPluginLoader _loader;
        private readonly TimeSpan _versionTtl;

        private TImpl _impl;
        private Stopwatch _ttl;

        #endregion

        #region private

        private TImpl load()
        {
            _impl = _loader.LoadImplementationOf(_likeThis, Ctor);
            _ttl = Stopwatch.StartNew();

            return _impl;
        }

        private TImpl cached()
        {
            var impl = _impl;
            var ttl = _ttl;

            bool isUpToDate =
                ttl.Elapsed < _versionTtl;

            if (isUpToDate)
                return impl;

            return null;
        }

        #endregion

        #region protected

        protected abstract TImpl Ctor();
        protected abstract void Dtor(TImpl impl);

        protected PluginOf(Type likeThis, TimeSpan versionTtl, IPluginLoader loader = null)
        {
            _likeThis = likeThis;
            _versionTtl = versionTtl;
            _loader = loader ?? PluginLoader.Instance;

            load();
        }

        protected void dispatch(Action<TImpl> call)
        {
            var impl = 
                cached() ?? load();

            call(impl);
        }

        protected R dispatch<R>(Func<TImpl, R> call)
        {
            var impl =
                cached() ?? load();

            return
                call(impl);
        }

        #endregion
    }
}