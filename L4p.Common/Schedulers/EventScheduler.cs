using System;
using System.Threading;
using L4p.Common.Extensions;
using L4p.Common.ForeverThreads;
using L4p.Common.Helpers;
using L4p.Common.Loggers;

namespace L4p.Common.Schedulers
{
    public interface IEventScheduler : IShouldBeStarted
    {
        void Start();
        void Stop();

        IEventSlot FireOnce(TimeSpan delay, Action action);
        IEventSlot Repeat(TimeSpan? periodOrNull, Action action);
        IEventSlot Schedule(EventInfo info, Action action);
        IEventSlot Idle(Action action);

        void CancelEvent(IEventSlot slot);
        void PauseEvent(IEventSlot slot);
        void ResumeEvent(IEventSlot slot);
    }

    public class EventScheduler : IEventScheduler
    {
        #region members

        private readonly ILogFile _log;
        private readonly ISchedulerRepo _repo;
        private readonly IForeverThread _thr;
        private readonly AutoResetEvent _hasNewTriger;

        #endregion

        #region construction

        public static IEventScheduler New(ILogFile log)
        {
            return
                new EventScheduler(log);
        }

        private EventScheduler(ILogFile log)
        {
            _log = log;
            _repo = SchedulerRepo.NewSync();
            _thr = ForeverThread.New(thread_loop, log);
            _hasNewTriger = new AutoResetEvent(false);
        }

        #endregion

        #region private

        private IEventSlot register_event(EventInfo info)
        {
            var slot = EventSlot.New(this);
            _repo.Add(info);

            _hasNewTriger.Set();

            return slot;
        }

        private void invoke_event(EventInfo info)
        {
            try
            {
                info.Action();
                info.Invoked++;
            }
            catch (Exception ex)
            {
                info.Failed++;
                info.LastFailure = ex;
                _log.Error(ex);
            }

            info.Count--;
        }

        private void schedule_next_shot(EventInfo info)
        {
            DateTime now = DateTime.UtcNow;
            info.NextShot = now + info.RepeatAfter;
        }

        private bool remove_if_done(EventInfo info)
        {
            if (info.Count > 0)
                return false;

            _repo.Remove(info);
            return true;
        }

        private void thread_loop()
        {
            while (true)
            {
                if (_thr.StopRequestIsPosted())
                    break;

                var now = DateTime.UtcNow;
                var events = _repo.GetDueEvents(now);

                foreach (var info in events)
                {
                    invoke_event(info);

                    if (!remove_if_done(info))
                        schedule_next_shot(info);

                    if (_thr.StopRequestIsPosted())
                        break;
                }

                _hasNewTriger.WaitOne(1);
            }
        }

        #endregion

        #region IEventScheduler

        void IEventScheduler.Start()
        {
            _thr.Start(this.GetType().Name);
        }

        void IEventScheduler.Stop()
        {
            _thr.Stop();
        }

        IEventSlot IEventScheduler.FireOnce(TimeSpan delay, Action action)
        {
            Validate.NotNull(action);

            var now = DateTime.UtcNow;

            var info = new EventInfo
                {
                    NextShot = now + delay,
                    Count = 1,
                    Action = action
                };

            var slot = register_event(info);

            return slot;
        }

        IEventSlot IEventScheduler.Repeat(TimeSpan? periodOrNull, Action action)
        {
            if (periodOrNull == null)
                return EventSlot.Null;

            var period = periodOrNull.Value;

            Validate.NotNull(action);
            Validate.That(period > TimeSpan.Zero, "Repeat with zero interval is not allowed");

            var now = DateTime.UtcNow;

            var info = new EventInfo {
                NextShot = now + period,
                RepeatAfter = period,
                Count = int.MaxValue,
                Action = action
            };

            var slot = register_event(info);

            return slot;
        }

        IEventSlot IEventScheduler.Schedule(EventInfo info, Action action)
        {
            Validate.NotNull(action);

            info.Action = action;

            var slot = register_event(info);

            return slot;
        }

        IEventSlot IEventScheduler.Idle(Action action)
        {
            Validate.NotNull(action);

            var info = new EventInfo
                {
                    RepeatAfter = 1.Milliseconds(),
                    Count = int.MaxValue,
                    Action = action
                };

            var slot = register_event(info);

            return slot;
        }

        void IEventScheduler.CancelEvent(IEventSlot slot)
        {
            throw new NotImplementedException();
        }

        void IEventScheduler.PauseEvent(IEventSlot slot)
        {
            throw new NotImplementedException();
        }

        void IEventScheduler.ResumeEvent(IEventSlot slot)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}