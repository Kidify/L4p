namespace L4p.Common.Schedulers
{
    public interface IEventSlot
    {
        void Cancel();
        void Pause();
        void Resume();
    }

    class EventSlot : IEventSlot
    {
        #region members

        private readonly IEventScheduler _scheduler;

        #endregion

        #region construction

        public static IEventSlot New(IEventScheduler scheduler)
        {
            return
                new EventSlot(scheduler);
        }

        private EventSlot(IEventScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        #endregion

        #region private
        #endregion

        #region IEventSlot

        void IEventSlot.Cancel()
        {
            _scheduler.CancelEvent(this);
        }

        void IEventSlot.Pause()
        {
            _scheduler.PauseEvent(this);
        }

        void IEventSlot.Resume()
        {
            _scheduler.ResumeEvent(this);
        }

        #endregion

        #region null

        private class NullSlot : IEventSlot
        {
            void IEventSlot.Cancel() { }
            void IEventSlot.Pause() { }
            void IEventSlot.Resume() { }
        }

        public static readonly IEventSlot Null = new NullSlot();

        #endregion
    }
}