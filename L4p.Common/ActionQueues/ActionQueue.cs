using System;
using System.Collections.Generic;

namespace L4p.Common.ActionQueues
{
    public interface IActionQueue
    {
        int Count { get; }
        int Push(Action action);
        Action Pop();
        void Clear();
    }

    public class ActionQueue : IActionQueue
    {
        #region members

        private readonly Queue<Action> _que;

        #endregion 

        #region construction

        public static IActionQueue New()
        {
            return 
                new ActionQueue();
        }

        private ActionQueue()
        {
            _que = new Queue<Action>();
        }

        #endregion

        #region private
        #endregion

        #region IActionQueue

        int IActionQueue.Count
        {
            get { return _que.Count; }
        }

        int IActionQueue.Push(Action action)
        {
            _que.Enqueue(action);
            int inQueue = _que.Count;

            return inQueue;
        }

        Action IActionQueue.Pop()
        {
            if (_que.Count == 0)
                return null;

            return
                _que.Dequeue();
        }

        void IActionQueue.Clear()
        {
            _que.Clear();
        }

        #endregion
    }

    class SyncedActionQueue : IActionQueue
    {
        private readonly object _mutex = new Object();
        private readonly IActionQueue _impl;

        public static IActionQueue New(IActionQueue impl) { return new SyncedActionQueue(impl); }
        private SyncedActionQueue(IActionQueue impl) { _impl = impl; }

        int IActionQueue.Count { get { lock(_mutex) return _impl.Count; }}

        int IActionQueue.Push(Action action) { lock (_mutex) return _impl.Push(action); }
        Action IActionQueue.Pop() { lock (_mutex) return _impl.Pop(); }
        void IActionQueue.Clear() { lock(_mutex) _impl.Clear(); }
    }
}