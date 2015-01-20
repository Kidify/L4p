using System;
using L4p.Common.Concerns;
using L4p.Common.FunnelsModel.comm;
using L4p.Common.Loggers;

namespace L4p.Common.FunnelsModel.client
{
    class FunnelStoreErrorHandler 
        : ErrorHandlerOf<IFunnelStore>, IFunnelStore
    {
        #region members

        private bool _isStopped;

        #endregion

        #region construction

        public static IFunnelStore New(ILogFile log, IFunnelStore impl)
        {
            return
                new FunnelStoreErrorHandler(log, impl);
        }

        private FunnelStoreErrorHandler(ILogFile log, IFunnelStore impl)
            : base(log, impl, (cause, msg, args) => new FunnelsException(cause, msg, args))
        {
            _isStopped = false;
        }

        #endregion

        #region private

        private void ensure_active_state()
        {
            if (_isStopped == false)
                return;

            throw 
                new FunnelsException("FunnelStore {0} is stopped", _impl.StoreId);
        }

        #endregion

        #region IFunnelStore

        Post IFunnelStore.GetPost(string tag)
        {
            ensure_active_state();

            return try_catch_wrap(
                () => _impl.GetPost(tag), 
                "tag='{0}'", tag);
        }

        void IFunnelStore.PublishPost(Post post)
        {
            ensure_active_state();

            try_catch_wrap(
                () => _impl.PublishPost(post),
                "path='{0}'", post.Path);
        }

        void IFunnelStore.PruneFunnelData(string path)
        {
            _impl.PruneFunnelData(path);
        }

        void IFunnelStore.Stop()
        {
            _isStopped = true;
            _impl.Stop();
        }

        Guid IFunnelStore.StoreId
        {
            get { return _impl.StoreId; }
        }

        string IFunnelStore.FunnelId
        {
            get { return _impl.FunnelId; }
            set { _impl.FunnelId = value; }
        }

        DateTime IFunnelStore.DeadAt
        {
            get { return _impl.DeadAt; }
            set { _impl.DeadAt = value; }
        }

        #endregion
    }
}