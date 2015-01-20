using System;
using System.Collections.Generic;
using L4p.Common.Concerns;
using L4p.Common.FunnelsModel.comm;

namespace L4p.Common.FunnelsModel.client
{
    interface IFunnelCache
    {
        Post Read(string path);
        void Write(string path, Post post);
        void Prune(string path);
        void Clear();
    }

    class FunnelCache : IFunnelCache
    {
        #region members

        private readonly Dictionary<string, Post> _items;

        #endregion

        #region construction

        public static IFunnelCache New()
        {
            return
                new FunnelCache();
        }

        private FunnelCache()
        {
            _items = new Dictionary<string, Post>();
        }

        #endregion

        #region private
        #endregion

        #region IFunnelCache

        Post IFunnelCache.Read(string path)
        {
            Post post;

            _items.TryGetValue(path, out post);
            return post;
        }

        void IFunnelCache.Write(string path, Post post)
        {
            _items[path] = post;
        }

        void IFunnelCache.Prune(string path)
        {
            _items.Remove(path);
        }

        void IFunnelCache.Clear()
        {
            _items.Clear();
        }

        #endregion
    }

    class MReadSWriteCache : ManyReadersSingleWriter<IFunnelCache>, IFunnelCache
    {
        public static IFunnelCache New(IFunnelCache impl) { return new MReadSWriteCache(impl); }
        private MReadSWriteCache(IFunnelCache impl) : base(impl) { }

        Post IFunnelCache.Read(string path) { return using_read_lock(() => _impl.Read(path)); }
        void IFunnelCache.Write(string path, Post post) { using_write_lock(() => _impl.Write(path, post)); }
        void IFunnelCache.Prune(string path) { using_write_lock(() => _impl.Prune(path)); }
        void IFunnelCache.Clear() { using_write_lock(() => _impl.Clear()); }
    }
}