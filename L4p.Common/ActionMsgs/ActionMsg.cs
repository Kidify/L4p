using System;
using System.Threading;

namespace L4p.Common.ActionMsgs
{
    public interface IActionMsg
    {
        void Run();
        void Join();
        void Join(TimeSpan timeout);
    }

    public class ActionMsg : IActionMsg
    {
        #region members

        private readonly Action _action;
        private readonly ManualResetEvent _ready;

        private Exception _ex;

        #endregion

        #region construction

        public static IActionMsg New(Action action)
        {
            return
                new ActionMsg(action);
        }

        public static IActionMsg<R> New<R>(Func<R> func)
        {
            return
                new ActionMsg<R>(func);
        }

        private ActionMsg(Action action)
        {
            _action = action;
            _ready = new ManualResetEvent(false);
        }

        #endregion

        #region private
        #endregion

        #region IActionMsg

        void IActionMsg.Run()
        {
            try
            {
                _action();
            }
            catch (Exception ex)
            {
                _ex = ex;
            }
            finally
            {
                _ready.Set();
            }
        }

        void IActionMsg.Join()
        {
            _ready.WaitOne();

            if (_ex != null)
                throw new ActionMsgException(_ex, "ActionMsg has thrown exception; {0}", _ex.Message);
        }

        void IActionMsg.Join(TimeSpan timeout)
        {
            bool done = _ready.WaitOne(timeout);

            if (!done)
                throw new ActionMsgException("ActionMsg timeout; timeout={0}", timeout);

            if (_ex != null)
                throw new ActionMsgException(_ex, "ActionMsg has thrown exception; {0}", _ex.Message);
        }

        #endregion
    }
}