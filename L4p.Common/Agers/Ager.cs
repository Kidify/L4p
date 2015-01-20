using System;
using System.Collections.Generic;

namespace L4p.Common.Agers
{
    public interface IAger<T>
        where T : class
    {
        void Add(T item);
        T GetExpiredItem(DateTime now, TimeSpan expirationSpan);
    }

    public class Ager<T> : IAger<T>
        where T : class
    {
        #region members

        private readonly Queue<T> _que;
        private readonly Func<T, DateTime> _ageGetter;

        #endregion

        #region construction

        public static IAger<T> New(Func<T, DateTime> ageGetter)
        {
            return
                new Ager<T>(ageGetter);
        }

        public static IAger<T> NewSync(Func<T, DateTime> ageGetter)
        {
            return
                SyncAger<T>.New(
                new Ager<T>(ageGetter));
        }

        private Ager(Func<T, DateTime> ageGetter)
        {
            _que = new Queue<T>();
            _ageGetter = ageGetter;
        }

        #endregion

        #region private
        #endregion

        #region IAger
        #endregion

        void IAger<T>.Add(T item)
        {
            _que.Enqueue(item);
        }

        T IAger<T>.GetExpiredItem(DateTime now, TimeSpan expirationSpan)
        {
            if (_que.Count == 0)
                return default(T);

            var item = _que.Peek();
            var age = _ageGetter(item);

            bool itemIsOld =
                age + expirationSpan < now;

            if (itemIsOld == false)
                return default(T);

            _que.Dequeue();

            return item;
        }
    }

    public class SyncAger<T> : IAger<T>
        where T : class
    {
        private readonly object _mutex = new object();
        private readonly IAger<T> _impl;

        public static IAger<T> New(IAger<T> impl) { return new SyncAger<T>(impl); }
        private SyncAger(IAger<T> impl) { _impl = impl; }

        void IAger<T>.Add(T item) { lock(_mutex) _impl.Add(item); }
        T IAger<T>.GetExpiredItem(DateTime now, TimeSpan expirationSpan) { lock(_mutex) return _impl.GetExpiredItem(now, expirationSpan); }
    }

}