using System;
using System.Threading;
using L4p.Common.Extensions;
using L4p.Common.Loggers;

namespace L4p.Common.ForeverThreads
{
    public interface IForeverThread
    {
        void Start(string name = null);
        void Stop();
        void PostStopRequest();
        bool StopRequestIsPosted();
        bool IsStopped();
        bool ItsMyThread();
        void Idle();
    }

    public class ForeverThread : IForeverThread
    {
        #region members

        private static int _threadCounter = 1;

        private readonly ILogFile _log;
        private readonly ForeverThreadConfig _config;
        private long _stopRequestCount;
        private readonly ManualResetEvent _isStartedEvent;
        private readonly ManualResetEvent _isStoppedEvent;
        private readonly Thread _thr;

        #endregion

        #region construction

        public static IForeverThread New(Action callback, ILogFile log = null, ForeverThreadConfig config = null)
        {
            log = log ?? LogFile.Console;
            config = config ?? new ForeverThreadConfig();

            return
                new ForeverThread(callback, log, config);
        }

        private ForeverThread(Action callback, ILogFile log, ForeverThreadConfig config)
        {
            _log = log;

            int count = Interlocked.Increment(ref _threadCounter);

            if (config == null)
            {
                config = new ForeverThreadConfig();
            }

            _config = new ForeverThreadConfig
                {
                    Name = config.Name ?? "ForeverThread.{0}".Fmt(count),
                    StartTimeout = config.StartTimeout,
                    StopTimeout = config.StopTimeout
                };

            _stopRequestCount = 0;
            _isStartedEvent = new ManualResetEvent(false);
            _isStoppedEvent = new ManualResetEvent(false);
            _thr = new Thread(() => thread_loop(callback));
        }

        #endregion

        #region private

        private void thread_loop(Action callback)
        {
            _isStartedEvent.Set();

            try
            {
                callback();
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }

            _isStoppedEvent.Set();
        }

        #endregion

        #region interface

        void IForeverThread.Start(string name)
        {
            if (name.IsNotEmpty())
                _config.Name = name;

            _thr.Start();
            bool isStarted = _isStartedEvent.WaitOne(_config.StartTimeout);

            if (isStarted)
                return;

            throw new ForeverThreadException(
                "Failed to start thread '{0}' in {1} milliseconds", _config.Name, _config.StartTimeout);
        }

        void IForeverThread.Stop()
        {
            Interlocked.Increment(ref _stopRequestCount);
            bool isStopped = _isStoppedEvent.WaitOne(_config.StopTimeout);

            if (isStopped)
                return;

            _log.Warn("Failed to stop thread '{0}' in {1} milliseconds", _config.Name, _config.StopTimeout);
        }

        void IForeverThread.PostStopRequest()
        {
            Interlocked.Increment(ref _stopRequestCount);
        }

        bool IForeverThread.StopRequestIsPosted()
        {
            bool posted = Interlocked.Read(ref _stopRequestCount) > 0;
            return posted;
        }

        bool IForeverThread.IsStopped()
        {
            bool isStopped = _isStoppedEvent.WaitOne(0);
            return isStopped;
        }

        bool IForeverThread.ItsMyThread()
        {
            return
                _thr == Thread.CurrentThread;
        }

        void IForeverThread.Idle()
        {
            if (_thr != Thread.CurrentThread)
                return;

            Thread.Sleep(1);
        }

        #endregion
    }
}