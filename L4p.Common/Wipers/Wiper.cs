using System;
using System.Collections.Generic;
using L4p.Common.Loggers;

namespace L4p.Common.Wipers
{
    public interface IWiper
    {
        event Action que;
        void Proceed();
    }

    public class Wiper : IWiper
    {
        #region members

        private readonly ILogFile _log;
        private readonly List<Action> _actions;

        #endregion

        #region construction

        public static IWiper New(ILogFile log = null)
        {
            return
                new Wiper(log);
        }

        private Wiper(ILogFile log)
        {
            _log = log ?? LogFile.New("clean-up.log");
            _actions = new List<Action>();
        }

        #endregion

        #region private

        private void add_clean_up(Action action)
        {
            _actions.Add(action);
        }

        private void invoke_clean_up(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        #endregion

        #region null

        private static readonly IWiper _null = new NullWiper();

        public static IWiper Null
        {
            get { return _null; }
        }

        #endregion

        #region interface

        event Action IWiper.que
        {
            add { add_clean_up(value); }
            remove { throw new ShouldNotBeCalledException(); }
        }

        void IWiper.Proceed()
        {
            var actions = _actions.ToArray();
            _actions.Clear();

            foreach (var action in actions)
            {
                invoke_clean_up(action);
            }
        }

        #endregion
    }
}