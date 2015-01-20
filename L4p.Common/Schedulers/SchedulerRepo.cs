using System;
using System.Collections.Generic;
using System.Linq;

namespace L4p.Common.Schedulers
{
    interface ISchedulerRepo
    {
        void Add(EventInfo info);
        void Remove(EventInfo info);
        EventInfo[] GetDueEvents(DateTime now);
    }

    class SchedulerRepo : ISchedulerRepo
    {
        #region members

        private readonly LinkedList<EventInfo> _events;

        #endregion

        #region construction

        public static ISchedulerRepo New()
        {
            return
                new SchedulerRepo();
        }

        public static ISchedulerRepo NewSync()
        {
            return
                SyncSchedulerRepo.New(
                new SchedulerRepo());
        }

        private SchedulerRepo()
        {
            _events = new LinkedList<EventInfo>();
        }

        #endregion

        #region private
        #endregion

        #region ISchedulerRepo
        #endregion

        void ISchedulerRepo.Add(EventInfo info)
        {
            _events.AddLast(info);
        }

        void ISchedulerRepo.Remove(EventInfo info)
        {
            _events.Remove(info);
        }

        EventInfo[] ISchedulerRepo.GetDueEvents(DateTime now)
        {
            var query =
                from info in _events
                where info.NextShot < now
                select info;

            return
                query.ToArray();
        }
    }

    class SyncSchedulerRepo : ISchedulerRepo
    {
        private readonly object _mutex = new object();
        private readonly ISchedulerRepo _impl;

        public static ISchedulerRepo New(ISchedulerRepo impl) { return new SyncSchedulerRepo(impl); }
        private SyncSchedulerRepo(ISchedulerRepo impl) { _impl = impl; }

        void ISchedulerRepo.Add(EventInfo info) { lock(_mutex) _impl.Add(info); }
        void ISchedulerRepo.Remove(EventInfo info) { lock(_mutex) _impl.Remove(info); }
        EventInfo[] ISchedulerRepo.GetDueEvents(DateTime now) { lock(_mutex) return _impl.GetDueEvents(now); }
    }
}