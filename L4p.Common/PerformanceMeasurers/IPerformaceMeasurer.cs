using System;
using System.Diagnostics;
using L4p.Common.Loggers;

namespace L4p.Common.PerformanceMeasurers
{
    public interface IPerformaceMeasurer
    {
        void TimeOf(ILogFile log, Action action);
        T TimeOf<T>(ILogFile log, Func<T> fucn);
    }

    public class PerformaceMeasurer : IPerformaceMeasurer
    {
        #region members

        private object _action;
        private object _func;
        
        #endregion

        #region construction

        public static IPerformaceMeasurer New()
        {
            return
                new PerformaceMeasurer();
        }

        private PerformaceMeasurer()
        {
        }

        #endregion

        #region private
        #endregion

        #region interface

        void IPerformaceMeasurer.TimeOf(ILogFile log, Action action)
        {
            var atype = action.GetType();

            if (_action != null && atype == _action.GetType())
                return;

            _action = action;

            var tm = Stopwatch.StartNew();

            try
            {
                action();
            }
            finally
            {
                _action = null;
                log.Trace("'{0}' took {1} milliseconds", action.Method.Name, tm.ElapsedMilliseconds);
            }
        }

        T IPerformaceMeasurer.TimeOf<T>(ILogFile log, Func<T> func)
        {
            var atype = func.GetType();

            if (_func != null && atype == _func.GetType())
                return default(T);

            _func = func;

            var tm = Stopwatch.StartNew();

            try
            {
                return func();
            }
            finally
            {
                _func = null;
                log.Trace("'{0}' took {1} milliseconds", func.Method.Name, tm.ElapsedMilliseconds);
            }
        }

        #endregion
    }
}