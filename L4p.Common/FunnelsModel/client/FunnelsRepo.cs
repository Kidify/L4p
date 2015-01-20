using System;
using System.Collections.Generic;
using System.Linq;
using L4p.Common.Agers;

namespace L4p.Common.FunnelsModel.client
{
    interface IFunnelsRepo
    {
        IFunnelStore Add(string funnelId, IFunnelStore store);
        void Remove(IFunnelStore store);
        IFunnelStore GetByFunnelId(string funnelId);
        IFunnelStore GetByStoreId(Guid storeId);
        IFunnelStore[] PopRemovedStores(DateTime now, TimeSpan ttl);
        IFunnelStore[] GetAllStores();
    }

    class FunnelsRepo : IFunnelsRepo
    {
        #region members

        private readonly Dictionary<string, IFunnelStore> _name2store;
        private readonly Dictionary<Guid, string> _id2name;
        private readonly Dictionary<Guid, IFunnelStore> _stores;
        private readonly IAger<IFunnelStore> _ager;

        #endregion

        #region construction

        public static IFunnelsRepo New()
        {
            return
                SyncedFunnelsRepo.New(
                new FunnelsRepo());
        }

        private FunnelsRepo()
        {
            _name2store = new Dictionary<string, IFunnelStore>();
            _id2name = new Dictionary<Guid, string>();
            _stores = new Dictionary<Guid, IFunnelStore>();
            _ager = Ager<IFunnelStore>.New(store => store.DeadAt);
        }

        #endregion

        #region private

        private void add_store(string funnelId, IFunnelStore store)
        {
            _name2store.Add(funnelId, store);
            _id2name.Add(store.StoreId, funnelId);
            _stores[store.StoreId] = store;
        }

        private void remove_store(IFunnelStore store)
        {
            var storeId = store.StoreId;
            var name = store.FunnelId;

            _name2store.Remove(name);
            _stores.Remove(storeId);
            _id2name.Remove(storeId);
        }

        private void pop_removed_stores(DateTime now, TimeSpan ttl, List<IFunnelStore> list)
        {
            while (true)
            {
                var store = _ager.GetExpiredItem(now, ttl);

                if (store == null)
                    break;

                remove_store(store);
                list.Add(store);
            }
        }

        #endregion

        #region IFunnelsRepo

        IFunnelStore IFunnelsRepo.Add(string funnelId, IFunnelStore store)
        {
            IFunnelStore prev;

            if (_name2store.TryGetValue(funnelId, out prev))
                return prev;

            add_store(funnelId, store);

            return store;
        }

        void IFunnelsRepo.Remove(IFunnelStore store)
        {
            _ager.Add(store);
        }

        IFunnelStore IFunnelsRepo.GetByFunnelId(string funnelId)
        {
            IFunnelStore store;

            _name2store.TryGetValue(funnelId, out store);

            return store;
        }

        IFunnelStore IFunnelsRepo.GetByStoreId(Guid storeId)
        {
            IFunnelStore store;

            _stores.TryGetValue(storeId, out store);

            return store;
        }

        IFunnelStore[] IFunnelsRepo.PopRemovedStores(DateTime now, TimeSpan ttl)
        {
            var list = new List<IFunnelStore>();
            pop_removed_stores(now, ttl, list);

            return list.ToArray();
        }

        IFunnelStore[] IFunnelsRepo.GetAllStores()
        {
            var query =
                from repo in _stores.Values select repo;

            return
                query.ToArray();
        }

        #endregion
    }

    class SyncedFunnelsRepo : IFunnelsRepo
    {
        private readonly object _mutex = new object();
        private readonly IFunnelsRepo _impl;

        public static IFunnelsRepo New(IFunnelsRepo impl) { return new SyncedFunnelsRepo(impl); }
        private SyncedFunnelsRepo(IFunnelsRepo impl) { _impl = impl; }

        IFunnelStore IFunnelsRepo.Add(string funnelId, IFunnelStore store) { lock (_mutex) return _impl.Add(funnelId, store); }
        void IFunnelsRepo.Remove(IFunnelStore store) { lock (_mutex) _impl.Remove(store); }
        IFunnelStore IFunnelsRepo.GetByFunnelId(string funnelId) { lock(_mutex) return _impl.GetByFunnelId(funnelId); }
        IFunnelStore IFunnelsRepo.GetByStoreId(Guid storeId) { lock (_mutex) return _impl.GetByStoreId(storeId); }
        IFunnelStore[] IFunnelsRepo.PopRemovedStores(DateTime now, TimeSpan ttl) { lock (_mutex) return _impl.PopRemovedStores(now, ttl); }
        IFunnelStore[] IFunnelsRepo.GetAllStores() { lock (_mutex) return _impl.GetAllStores(); }
    }
}