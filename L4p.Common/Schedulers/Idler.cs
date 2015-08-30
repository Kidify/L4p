using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using L4p.Common.DumpToLogs;
using L4p.Common.Helpers;
using L4p.Common.Extensions;
using L4p.Common.InsightCounters;
using L4p.Common.Loggers;

namespace L4p.Common.Schedulers
{
    public interface IIdler : IHaveDump
    {
        void Stop();
        void Idle(TimeSpan period, Action action);
    }

    class IdlerAction
    {
        public Action Action { get; set; }
        public Timer Timer { get; set; }
        public TimeSpan Period { get; set; }
        public int Mutex;
        public long SuccessfulExecutions;
        public long FailedExecutions;
        public long SkippedExecutions;
    }

    public class Idler : IIdler
    {
        #region counters

        class Counters
        {
            public long JobsExcecuted;
            public long JobsFailed;
            public long JobsSkipped;
        }

        #endregion

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

        private readonly Counters _counters;
        private readonly ILogFile _log;
        private readonly Config _config;
        private IdlerAction[] _jobs;

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
            _counters = MyCounters.New<Counters>(log);
            _config = config;
            _jobs = new IdlerAction[0];
        }

        #endregion

        #region private

        private IdlerAction new_job(Action action, TimeSpan period)
        {
            return new IdlerAction {
                Action = action,
                Period = period,
                Mutex = 0
            };
        }

        private void idle_job(IdlerAction job)
        {
            int concurrentJobs = Interlocked.Increment(ref job.Mutex);

            try
            {
                if (concurrentJobs > 1)
                {
                    Interlocked.Increment(ref job.SkippedExecutions);
                    Interlocked.Increment(ref _counters.JobsSkipped);
                    return;
                }

                job.Action();
                Interlocked.Increment(ref job.SuccessfulExecutions);
                Interlocked.Increment(ref _counters.JobsExcecuted);
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref job.FailedExecutions);
                Interlocked.Increment(ref _counters.JobsFailed);
                _log.Error(ex);
            }
            finally
            {
                Interlocked.Decrement(ref job.Mutex);
            }
        }

        private IdlerAction start_job(TimeSpan period, Action action)
        {
            var job = new_job(action, period);

            job.Timer = new Timer(
                obj => idle_job(job), null, period, period);

            return job;
        }

        private void stop_job(IdlerAction job, TimeSpan stopSpan)
        {
            using (var stopEvent = new ManualResetEvent(false))
            {
                job.Timer.Dispose(stopEvent);
                stopEvent.WaitOne(stopSpan);
            }
        }

        private void add_job(IdlerAction job)
        {
            lock (this)
            {
                var list = new List<IdlerAction>(_jobs);
                list.Add(job);

                _jobs = list.ToArray();
            }
        }

        #endregion

        #region interface

        ExpandoObject IHaveDump.Dump(dynamic root)
        {
            if (root == null)
                root = new ExpandoObject();

            var jobs = 
                from job in _jobs
                select "Period={0} mutex={1} [ok={2} err={3} skip={4}]".Fmt(
                    job.Period, job.Mutex, job.SkippedExecutions, job.FailedExecutions, job.SkippedExecutions);

            root.Config = _config;
            root.Counters = _counters;
            root.Jobs = jobs.ToArray();

            return root;
        }

        void IIdler.Stop()
        {
            var timers = _jobs;
            _jobs = null;

            if (timers.IsEmpty())
                return;

            foreach (var timer in timers)
            {
                Try.Catch.Handle(
                    () => stop_job(timer, _config.StopSpan),
                    ex => _log.Warn("Failed to stop a timer: {0}", ex.Message));
            }
        }

        void IIdler.Idle(TimeSpan period, Action action)
        {
            var timer = start_job(period, action);
            add_job(timer);
        }

        #endregion
    }
}