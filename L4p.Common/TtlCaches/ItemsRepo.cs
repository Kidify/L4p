using System;
using System.Collections.Generic;
using System.Linq;

namespace L4p.Common.TtlCaches
{
    interface IItemsRepo<TBody>
        where TBody : class
    {
        ITtlItem<TBody> GetBy(TBody body);
        ITtlItem<TBody> Add(ITtlItem<TBody> item);
        ITtlItem<TBody>[] GetDeadItems(TimeSpan ttl);
        void Remove(ITtlItem<TBody>[] items);
    }

    class ItemsRepo<TBody> : IItemsRepo<TBody>
        where TBody : class
    {
        #region members

        private readonly Dictionary<TBody, ITtlItem<TBody>> _items;

        #endregion

        #region construction

        public static IItemsRepo<TBody> New()
        {
            return
                SyncedItemsRepo<TBody>.New(
                new ItemsRepo<TBody>());
        }

        private ItemsRepo()
        {
            _items = new Dictionary<TBody, ITtlItem<TBody>>();
        }

        #endregion

        #region private
        #endregion

        #region IItemsRepo

        ITtlItem<TBody> IItemsRepo<TBody>.GetBy(TBody body)
        {
            ITtlItem<TBody> item;
            _items.TryGetValue(body, out item);

            return item;
        }

        ITtlItem<TBody> IItemsRepo<TBody>.Add(ITtlItem<TBody> item)
        {
            ITtlItem<TBody> prev;

            if (_items.TryGetValue(item.Body, out prev))
                return prev;

            _items.Add(item.Body, item);

            return item;
        }

        ITtlItem<TBody>[] IItemsRepo<TBody>.GetDeadItems(TimeSpan ttl)
        {
            var now = DateTime.UtcNow;

            var query =
                from item in _items.Values
                where now - item.DeadSince > ttl
                select item;

            return
                query.ToArray();
        }

        void IItemsRepo<TBody>.Remove(ITtlItem<TBody>[] items)
        {
            if (items == null)
                return;

            foreach (var item in items)
            {
                _items.Remove(item.Body);
            }
        }

        #endregion
    }

    class SyncedItemsRepo<TBody> : IItemsRepo<TBody> 
        where TBody : class
    {
        private readonly object _mutex = new object();
        private readonly IItemsRepo<TBody> _impl;

        public static IItemsRepo<TBody> New(IItemsRepo<TBody> impl) { return new SyncedItemsRepo<TBody>(impl); }
        private SyncedItemsRepo(IItemsRepo<TBody> impl) { _impl = impl; }
            
        ITtlItem<TBody> IItemsRepo<TBody>.GetBy(TBody body) { lock(_mutex) return _impl.GetBy(body); }
        ITtlItem<TBody> IItemsRepo<TBody>.Add(ITtlItem<TBody> item) { lock(_mutex) return _impl.Add(item); }
        ITtlItem<TBody>[] IItemsRepo<TBody>.GetDeadItems(TimeSpan ttl) { lock(_mutex) return _impl.GetDeadItems(ttl); }
        void IItemsRepo<TBody>.Remove(ITtlItem<TBody>[] items) { lock(_mutex) _impl.Remove(items); }
    }
}