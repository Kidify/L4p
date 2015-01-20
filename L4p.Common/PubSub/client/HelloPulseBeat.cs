using System;
using System.Dynamic;
using System.Threading;
using L4p.Common.DumpToLogs;
using L4p.Common.Extensions;
using L4p.Common.IoCs;
using L4p.Common.Loggers;

namespace L4p.Common.PubSub.client
{
    interface IHeloPulseBeat : IHaveDump
    {
        void Start(ISignalsManagerEx signals);
        void Stop();
        void SayHelloIn(TimeSpan soon);
    }

    class HelloPulseBeat : IHeloPulseBeat
    {
        #region config

        public class Config
        {
            public TimeSpan FirstTimeout { get; set; }
            public TimeSpan StopTimeout { get; set; }

            public Config()
            {
                FirstTimeout = 500.Milliseconds();
                StopTimeout = 1.Seconds();
            }
        }

        #endregion

        #region members

        private readonly Config _myConfig;
        private readonly ISignalsManagerEx _signals;
        private readonly TimeSpan _period;
        private readonly ILogFile _log;
        private readonly Timer _timer;

        private int _sequentialId; 

        #endregion

        #region construction

        public static IHeloPulseBeat New(IIoC ioc, Config config = null)
        {
            return
                new HelloPulseBeat(ioc, config ?? new Config());
        }

        private HelloPulseBeat(IIoC ioc, Config myConfig)
        {
            _myConfig = myConfig;

            _signals = ioc.Resolve<ISignalsManagerEx>();

            var configRa = ioc.Resolve<ISignalsConfigRa>();
            var config = configRa.Values;

            _log = ThrottledLog.NewSync(config.ThrottledLogTtl, ioc.Resolve<ILogFile>());

            _period = config.Client.HelloMsgPeriod;
            _timer = new Timer(pulse);
        }

        #endregion

        #region private

        private void pulse(object state)
        {
            try
            {
                var sequentialId = Interlocked.Increment(ref _sequentialId);
                _signals.GenerateHelloMsg(this, sequentialId);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Exception while pulse beat");
            }
        }

        private void stop_timer()
        {
            var stopEvent = new ManualResetEvent(false);

            _timer.Dispose(stopEvent);
            stopEvent.WaitOne(_myConfig.StopTimeout);
        }

        #endregion

        #region interface

        void IHeloPulseBeat.Start(ISignalsManagerEx signals)
        {
            _timer.Change(_myConfig.FirstTimeout, _period);
            _log.Trace("PulseBeat is started period='{0}'", _period);
        }

        void IHeloPulseBeat.Stop()
        {
            try
            {
                stop_timer();
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Failed to stop pulse beat timer");
            }
        }

        void IHeloPulseBeat.SayHelloIn(TimeSpan soon)
        {
            _timer.Change(soon, _period);
        }

        ExpandoObject IHaveDump.Dump(dynamic root)
        {
            if (root == null)
                root = new ExpandoObject();

            root.MyConfig = _myConfig;
            root.Period = _period;

            return root;
        }

        #endregion
    }
}