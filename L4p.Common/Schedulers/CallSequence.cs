using System;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;
using L4p.Common.DumpToLogs;
using L4p.Common.Loggers;

namespace L4p.Common.Schedulers
{
    public interface ICallSequence : IHaveDump
    {
        void Enqueue(Action action);
    }

    public class CallSequence : ICallSequence
    {
        #region counters

        class Counters
        {
            public int Enqueue;
            public int Invoked;
            public int Failed;
        }

        #endregion

        #region members

        private readonly Counters _counters;
        private readonly ILogFile _log;

        private Task _tail;

        #endregion

        #region construction

        public static ICallSequence New(ILogFile log)
        {
            return
                new CallSequence(log);
        }

        private CallSequence(ILogFile log)
        {
            _counters = new Counters();
            _log = log;
            _tail = null;
        }

        #endregion

        #region private

        private void invoke(Action action)
        {
            try
            {
                action();
                Interlocked.Increment(ref _counters.Invoked);
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref _counters.Failed);
                _log.Error(ex);
            }
        }

        #endregion

        #region interface

        ExpandoObject IHaveDump.Dump(dynamic root)
        {
            if (root == null)
                root = new ExpandoObject();

            root.Counters = _counters;

            return root;
        }

        void ICallSequence.Enqueue(Action action)
        {
            Interlocked.Increment(ref _counters.Enqueue);

            var next = new Task(() => invoke(action));
            var prev = Interlocked.Exchange(ref _tail, next);

            if (prev == null)
            {
                next.Start();
                return;
            }

            prev.ContinueWith(x => next.Start());
        }

        #endregion
    }
}