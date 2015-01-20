using System;
using L4p.Common.FunnelsModel.comm;
using L4p.Common.Loggers;

namespace L4p.Common.FunnelsModel.client
{
    interface IFunnelStore
    {
        Post GetPost(string tag);
        void PublishPost(Post post);
        void PruneFunnelData(string path);
        void Stop();

        Guid StoreId { get; }
        string FunnelId { get; set; }
        DateTime DeadAt { get; set; }
    }

    class FunnelStore : IFunnelStore
    {
        #region members

        private readonly Guid _storeId;
        private readonly IFunnelCache _cache;
        private readonly IFunnelsShop _shop;

        #endregion

        #region construction

        public static IFunnelStore New(Guid storeId, IFunnelsShop shop, ILogFile log)
        {
            return
                FunnelStoreErrorHandler.New(log,
                new FunnelStore(storeId, shop));
        }

        private FunnelStore(Guid storeId, IFunnelsShop shop)
        {
            _storeId = storeId;
            _cache = MReadSWriteCache.New(FunnelCache.New());
            _shop = shop;
        }

        #endregion

        #region private
        #endregion

        #region interface

        Post IFunnelStore.GetPost(string path)
        {
            var post = _cache.Read(path);

            if (post != null)
                return post;

            post = _shop.GetPost(_storeId, path);

            if (post == null)
                return null;

            _cache.Write(path, post);

            return post;
        }

        void IFunnelStore.PublishPost(Post post)
        {
            _cache.Write(post.Path, post);
            _shop.PublishPost(_storeId, post);
        }

        void IFunnelStore.PruneFunnelData(string path)
        {
            _cache.Prune(path);
        }

        void IFunnelStore.Stop()
        {
            _cache.Clear();
            _shop.StoreIsRemoved(_storeId);
            _shop.CloseConnection();
        }

        Guid IFunnelStore.StoreId
        {
            get { return _storeId; }
        }

        DateTime IFunnelStore.DeadAt { get; set; }
        string IFunnelStore.FunnelId { get; set; }

        #endregion
    }
}