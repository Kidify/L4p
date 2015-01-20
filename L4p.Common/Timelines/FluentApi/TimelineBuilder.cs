using System;
using L4p.Common.Helpers;

namespace L4p.Common.Timelines.FluentApi
{
    public interface ITimelineBuilder
    {
        ITimelineBuilder Now(Action action);
        ITimelineBuilder Now(Action<DateTime> action);

        ITimelineBuilder Then(Action action);
        ITimelineBuilder Then(Action<DateTime> action);

        ITimelineBuilder Wait(TimeSpan span);
        ITimelineBuilder Wait(Func<DateTime> getTime);

        ITimelineBuilder Before(TimeSpan before, Action<DateTime> action);
        ITimelineBuilder After(TimeSpan after, Action<DateTime> action);

        void Run();
        void Run(DateTime now);
    }

    class TimelineBuilder : ITimelineBuilder
    {
        #region members

        private readonly ITimeline _timeline;
        private readonly Random _rnd;

        #endregion

        #region construction

        public static ITimelineBuilder New(ITimeline timeline)
        {
            return
                new TimelineBuilder(timeline);
        }

        private TimelineBuilder(ITimeline timeline)
        {
            _timeline = timeline;
            _rnd = new Random();
        }

        #endregion

        #region private
        #endregion

        #region ITimelineBuilder

        ITimelineBuilder ITimelineBuilder.Now(Action action)
        {
            _timeline.AddAction(tm => action(), TimeSpan.Zero);
            return this;
        }

        ITimelineBuilder ITimelineBuilder.Now(Action<DateTime> action)
        {
            _timeline.AddAction(action, TimeSpan.Zero);
            return this;
        }

        ITimelineBuilder ITimelineBuilder.Then(Action action)
        {
            var delay = TimeSpan.FromMilliseconds(_rnd.Next(100));
            _timeline.AddAction(tm => action(), delay);
            return this;
        }

        ITimelineBuilder ITimelineBuilder.Then(Action<DateTime> action)
        {
            var delay = TimeSpan.FromMilliseconds(_rnd.Next(100));
            _timeline.AddAction(action, delay);
            return this;
        }

        ITimelineBuilder ITimelineBuilder.Wait(TimeSpan span)
        {
            _timeline.AddAction(null, span);
            return this;
        }

        ITimelineBuilder ITimelineBuilder.Wait(Func<DateTime> getTime)
        {
            _timeline.AddDeferredBarrier(getTime);
            return this;
        }

        ITimelineBuilder ITimelineBuilder.Before(TimeSpan before, Action<DateTime> action)
        {
            _timeline.AddActionAt(action, -before);
            return this;
        }

        ITimelineBuilder ITimelineBuilder.After(TimeSpan after, Action<DateTime> action)
        {
            _timeline.AddActionAt(action, after);
            return this;
        }

        void ITimelineBuilder.Run()
        {
            _timeline.Run();
        }

        void ITimelineBuilder.Run(DateTime now)
        {
            _timeline.Run(now);
        }

        #endregion
    }
}