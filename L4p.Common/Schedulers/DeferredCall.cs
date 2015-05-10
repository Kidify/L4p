using System;
using System.Threading;
using System.Threading.Tasks;
using L4p.Common.Extensions;
using L4p.Common.Loggers;

namespace L4p.Common.Schedulers
{
    public interface IDeferredCall
    {
        void WaitForCompletion();
    }

    public class DeferredCall : IDeferredCall
    {
        #region members

        private readonly Action _action;
        private readonly Task _task;

        #endregion

        #region construction

        public static IDeferredCall New(Action action)
        {
            return
                new DeferredCall(action);
        }

        public static void Start(Action action)
        {
            Task.Factory.StartNew(action);
        }

        public static void Start(TimeSpan delay, Action action)
        {
            Task.Factory.StartNew(() => call_timed_action(delay, action));
        }

        private DeferredCall(Action action)
        {
            _action = action;
            _task = Task.Factory.StartNew(action);
        }

        #endregion

        #region private

        private static void call_timed_action(TimeSpan delay, Action action)
        {
            Thread.Sleep(delay);
            action();
        }

        #endregion

        #region interface

        void IDeferredCall.WaitForCompletion()
        {
            try
            {
                _task.Wait();
            }
            catch (Exception ex)
            {
                throw
                    ex.WrapWith<L4pException>("Deferred call to '{0}' failed", _action.Method.Name);
            }
        }

        #endregion
    }
}