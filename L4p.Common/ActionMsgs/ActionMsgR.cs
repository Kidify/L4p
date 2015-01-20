using System;
using System.Threading;

namespace L4p.Common.ActionMsgs
{
    public interface IActionMsg<R>
    {
        void Run();
        R Join();
        R Join(TimeSpan timeout);
    }

    public class ActionMsg<R> : IActionMsg<R>
    {
        #region members

        private readonly Func<R> _func;
        private readonly ManualResetEvent _ready;

        private R _result;
        private Exception _ex;

        #endregion

        #region construction

        internal ActionMsg(Func<R> func)
        {
            _func = func;
            _ready = new ManualResetEvent(false);
        }

        #endregion

        #region private
        #endregion

        #region IActionMsg

        void IActionMsg<R>.Run()
        {
            try
            {
                _result = _func();
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

        R IActionMsg<R>.Join()
        {
            _ready.WaitOne();

            if (_ex != null)
                throw new ActionMsgException(_ex, "ActionMsg has thrown exception; {0}", _ex.Message);

            return _result;
        }

        R IActionMsg<R>.Join(TimeSpan timeout)
        {
            bool done = _ready.WaitOne(timeout);

            if (!done)
                throw new ActionMsgException("ActionMsg timeout; timeout={0}", timeout);

            if (_ex != null)
                throw new ActionMsgException(_ex, "ActionMsg has thrown exception; {0}", _ex.Message);

            return _result;
        }

        #endregion
    }
}