using System;
using System.Threading;
using L4p.Common.ActionMsgs;
using L4p.Common.ActionQueues;
using L4p.Common.ForeverThreads;
using L4p.Common.Loggers;

namespace L4p.Common.ActiveObjects
{
    public interface IActivePeer
    {
        void Start(string thrName = null);
        void Stop();
    }

    public interface IActiveObject : IShouldBeStarted
    {
        void Start(string thrName = null);
        void Stop();
        int PushAction(Action action);
        AoCounters GetCounters { get; }
    }

    public class ActiveObject : IActiveObject, IActivePeer
    {
        #region members

        private readonly ILogFile _log;
        private readonly IActivePeer _impl;
        private readonly string _targetClass;
        private readonly IForeverThread _thr;
        private readonly IActionQueue _que;
        private readonly AoCounters _counters;

        private long _postedActionsCounter;
        private int _nextWarningInQueueCount;

        #endregion 

        #region construction

        protected ActiveObject(ILogFile log, IActivePeer impl = null)
        {
            _log = log;
            _impl = impl;
            _targetClass = (impl != null) ? impl.GetType().Name : "n/a";
            _thr = ForeverThread.New(active_loop, log);
            _que = SyncedActionQueue.New(ActionQueue.New());
            _counters = new AoCounters();
            _postedActionsCounter = 0;
            _nextWarningInQueueCount = 1000;
        }

        #endregion

        #region private 

        private void invoke(Action action)
        {
            _counters.Started++;

            try
            {
                action();
                _counters.Succeeded++;
            }
            catch
            {
                _counters.Failed++;
            }

            _counters.Completed++;
        }

        private int push_action(Action action)
        {
            int inQueue = _que.Push(action);

            if (inQueue > _nextWarningInQueueCount)
            {
                _nextWarningInQueueCount *= 2;
                TraceLogger.WriteLine("Warn: ActionQueueSeemsToBeStuck inQueue={0} Target class is '{1}' (nextWarning at {2})", inQueue, _targetClass, _nextWarningInQueueCount);
            }

            return inQueue;
        }

        private Action pop_action()
        {
            var action = _que.Pop();
            return action;
        }

        private void active_loop()
        {
            if (_impl != null)
                _impl.Start();

            while (true)
            {
                if (_thr.StopRequestIsPosted())
                    break;

                var action = pop_action();

                if (action == null)
                {
                    Idle();
                    continue;
                }

                invoke(action);
            }

            if (_impl != null)
                _impl.Stop();
        }

        #endregion

        #region protected

        private void validate_its_not_my_thread()
        {
            if (_thr.ItsMyThread())
                throw new ActiveObjectException("Active object method is called on its own thread");
        }

        protected virtual void Start(string thrName)
        {
            _thr.Start(thrName ?? _targetClass);
        }

        protected virtual void Stop()
        {
            _thr.Stop();
        }

        protected int InQueue
        {
            get { return _que.Count; }
        }

        protected virtual void Idle()
        {
            Thread.Sleep(1);
        }

        protected int dispatch(Action action)
        {
            bool isStopped =
                _thr.StopRequestIsPosted();

            if (isStopped)
                throw new ActiveObjectException("ActiveObject is stopped");

            int inQueue = push_action(action);

            Interlocked.Increment(ref _postedActionsCounter);

            return inQueue;
        }

        protected void join(TimeSpan timeout, Action action)
        {
            if (_thr.ItsMyThread())
            {
                _que.Push(action);
                _que.Run();

                return;
            }

            var msg = ActionMsg.New(action);
            push_action(msg.Run);

            msg.Join(timeout);
        }

        protected R join<R>(TimeSpan timeout, Func<R> func)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region interface

        void IActivePeer.Start(string thrName)
        {
            Start(thrName);
        }

        void IActivePeer.Stop()
        {
            Stop();
        }

        void IActiveObject.Start(string thrName)
        {
            Start(thrName);
        }

        void IActiveObject.Stop()
        {
            Stop();
        }

        int IActiveObject.PushAction(Action action)
        {
            int inQueue = dispatch(action);
            return inQueue;
        }

        AoCounters IActiveObject.GetCounters
        {
            get
            {
                int posted = (int) Interlocked.Read(ref _postedActionsCounter);

                return new AoCounters
                    {
                        Posted = posted,
                        Started = _counters.Started,
                        Completed = _counters.Completed,
                        Succeeded = _counters.Succeeded,
                        Failed = _counters.Failed
                    };
            }
        }

        #endregion
    }
}