using System;
using System.Threading;
using L4p.Common.ActionMsgs;
using L4p.Common.ActionQueues;

namespace L4p.Common.Concerns
{
    public interface IHasActionQueue
    {
        IActionQueue ActionQueue { get; }
    }

    public class SynchronizedCallsOf<T>
    {
        #region members

        protected readonly IActionQueue _que;

        protected int MyThreadId { get; set; }

        #endregion

        #region construction

        protected SynchronizedCallsOf(IActionQueue que = null)
        {
            if (que == null)
                que = ActionQueue.New();

            _que = SyncedActionQueue.New(que);
        }

        #endregion

        #region private

        private bool its_my_thread()
        {
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;

            return
                currentThreadId == MyThreadId;
        }

        protected void validate_its_my_thread()
        {
            int currentThreadId = Thread.CurrentThread.ManagedThreadId;

            if (currentThreadId == MyThreadId)
                return;

            throw new ConcernsException(
                "Method should be invoked within manager thread; currentThreadId={0} _myThreadId={1}", currentThreadId, MyThreadId);
        }


        #endregion

        #region protected

        protected int dispatch(Action action)
        {
            int inQueue = _que.Push(action);
            return inQueue;
        }

        protected void join(TimeSpan timeout, Action action)
        {
            if (its_my_thread())
            {
                action();
                return;
            }

            var msg = ActionMsg.New(action);
            _que.Push(msg.Run);

            msg.Join(timeout);
        }

        protected R join<R>(TimeSpan timeout, Func<R> func)
        {
            if (its_my_thread())
            {
                return
                    func();
            }

            throw new NotImplementedException();
        }

        #endregion
    }
}