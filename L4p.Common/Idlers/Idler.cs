using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using L4p.Common.Extensions;
using L4p.Common.Loggers;

namespace L4p.Common.Idlers
{
    interface IIdler
    {
        void Stop();
        void Idle(TimeSpan period, Action action);
    }

    class Idler : IIdler
    {
        #region config

        public class Config
        {
            public TimeSpan StopSpan { get; set; }

            public Config()
            {
                StopSpan = 500.Milliseconds();
            }
        }

        #endregion

        #region members

        private readonly ILogFile _log;
        private readonly Config _config;
        private Timer[] _timers;

        #endregion

        #region construction

        public static IIdler New(ILogFile log, Config config = null)
        {
            return
                new Idler(log, config ?? new Config());
        }

        private Idler(ILogFile log, Config config)
        {
            _log = log.WrapIfNull();
            _config = config;
            _timers = new Timer[0];
        }

        #endregion

        #region private

        private void idle(Action action)
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

        private Timer start_timer(TimeSpan period, Action action)
        {
            return new Timer(
                obj => idle(action), null, period, period);
        }

        private void stop_timer(Timer timer, TimeSpan stopSpan)
        {
            var stopEvent = new ManualResetEvent(false);

            timer.Dispose(stopEvent);
            stopEvent.WaitOne(stopSpan);
        }

        private void add_timer(Timer timer)
        {
            lock (this)
            {
                var list = new List<Timer>(_timers);
                list.Add(timer);

                _timers = list.ToArray();
            }
        }

        #endregion

        #region interface

        void IIdler.Stop()
        {
            var timers = _timers;
            _timers = null;

            if (timers.IsEmpty())
                return;

            foreach (var timer in timers)
            {
                stop_timer(timer, _config.StopSpan);
            }
        }

        void IIdler.Idle(TimeSpan period, Action action)
        {
            var timer = start_timer(period, action);
            add_timer(timer);
        }

        #endregion
    }
}