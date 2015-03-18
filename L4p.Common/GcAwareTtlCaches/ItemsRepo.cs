using System;
using System.Collections.Generic;
using System.Linq;

namespace L4p.Common.GcAwareTtlCaches
{
    interface IItemsRepo<TInstance>
        where TInstance : class
    {
        ITtlItem<TInstance> GetBy(TInstance instance);
        ITtlItem<TInstance> Add(ITtlItem<TInstance> item);
        ITtlItem<TInstance>[] GetDeadItems(TimeSpan ttl);
        void Remove(ITtlItem<TInstance>[] items);
    }

    class ItemsRepo<TInstance> : IItemsRepo<TInstance>
        where TInstance : class
    {
        #region members

        private readonly Dictionary<TInstance, ITtlItem<TInstance>> _items;

        #endregion

        #region construction

        public static IItemsRepo<TInstance> New()
        {
            return
                SyncedItemsRepo<TInstance>.New(
                new ItemsRepo<TInstance>());
        }

        private ItemsRepo()
        {
            _items = new Dictionary<TInstance, ITtlItem<TInstance>>();
        }

        #endregion

        #region private
        #endregion

        #region IItemsRepo

        ITtlItem<TInstance> IItemsRepo<TInstance>.GetBy(TInstance instance)
        {
            ITtlItem<TInstance> item;
            _items.TryGetValue(instance, out item);

            return item;
        }

        ITtlItem<TInstance> IItemsRepo<TInstance>.Add(ITtlItem<TInstance> item)
        {
            ITtlItem<TInstance> prev;

            if (_items.TryGetValue(item.Instance, out prev))
                return prev;

            _items.Add(item.Instance, item);

            return item;
        }

        ITtlItem<TInstance>[] IItemsRepo<TInstance>.GetDeadItems(TimeSpan ttl)
        {
            var now = DateTime.UtcNow;

            var query =
                from item in _items.Values
                where now - item.DeadSince > ttl
                select item;

            return
                query.ToArray();
        }

        void IItemsRepo<TInstance>.Remove(ITtlItem<TInstance>[] items)
        {
            if (items == null)
                return;

            foreach (var item in items)
            {
                _items.Remove(item.Instance);
            }
        }

        #endregion
    }

    class SyncedItemsRepo<TInstance> : IItemsRepo<TInstance> 
        where TInstance : class
    {
        private readonly object _mutex = new object();
        private readonly IItemsRepo<TInstance> _impl;

        public static IItemsRepo<TInstance> New(IItemsRepo<TInstance> impl) { return new SyncedItemsRepo<TInstance>(impl); }
        private SyncedItemsRepo(IItemsRepo<TInstance> impl) { _impl = impl; }
            
        ITtlItem<TInstance> IItemsRepo<TInstance>.GetBy(TInstance instance) { lock(_mutex) return _impl.GetBy(instance); }
        ITtlItem<TInstance> IItemsRepo<TInstance>.Add(ITtlItem<TInstance> item) { lock(_mutex) return _impl.Add(item); }
        ITtlItem<TInstance>[] IItemsRepo<TInstance>.GetDeadItems(TimeSpan ttl) { lock(_mutex) return _impl.GetDeadItems(ttl); }
        void IItemsRepo<TInstance>.Remove(ITtlItem<TInstance>[] items) { lock(_mutex) _impl.Remove(items); }
    }
}