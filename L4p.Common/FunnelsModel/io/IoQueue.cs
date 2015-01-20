using System.Collections.Generic;
using System.Linq;

namespace L4p.Common.FunnelsModel.io
{
    interface IIoQueue
    {
        void Push(Ioop ioop);
        void PushFront(Ioop ioop);
        void Complete(Ioop ioop);

        Ioop Get(Todo state);
        void Release(Ioop ioop);
    }

    class IoQueue : IIoQueue
    {
        #region members

        private readonly LinkedList<Ioop> _que;

        #endregion

        #region construction

        public static IIoQueue New()
        {
            return
                IoSyncQueue.New(new IoQueue());
        }

        private IoQueue()
        {
            _que = new LinkedList<Ioop>();
        }

        #endregion

        #region private

        #endregion

        #region interface

        void IIoQueue.Push(Ioop ioop)
        {
            _que.AddLast(ioop);
        }

        void IIoQueue.PushFront(Ioop ioop)
        {
            _que.AddFirst(ioop);
        }

        void IIoQueue.Complete(Ioop ioop)
        {
            _que.Remove(ioop);
        }

        Ioop IIoQueue.Get(Todo state)
        {
            return
                _que.FirstOrDefault(ioop => ioop.Next == state);
        }

        void IIoQueue.Release(Ioop ioop)
        {
        }

        #endregion
    }

    class IoSyncQueue : IIoQueue
    {
        private readonly object _mutex = new object();
        private readonly IIoQueue _impl;

        public static IIoQueue New(IIoQueue impl) { return new IoSyncQueue(impl); }
        private IoSyncQueue(IIoQueue impl) { _impl = impl; }

        void IIoQueue.Push(Ioop ioop) { lock(_mutex) _impl.Push(ioop); }
        void IIoQueue.PushFront(Ioop ioop) { lock(_mutex) _impl.PushFront(ioop); }
        void IIoQueue.Complete(Ioop ioop) { lock(_mutex) _impl.Complete(ioop); }

        Ioop IIoQueue.Get(Todo state) { lock(_mutex) return _impl.Get(state); }
        void IIoQueue.Release(Ioop ioop) { lock(_mutex) _impl.Release(ioop); }
    }
}