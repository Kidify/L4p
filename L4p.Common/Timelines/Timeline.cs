using System;
using System.Collections.Generic;

namespace L4p.Common.Timelines
{
    public interface ITimeline
    {
        void AddAction(Action<DateTime> action, TimeSpan delay);
        void AddActionAt(Action<DateTime> action, TimeSpan shift);
        void AddBarrier(DateTime time);
        void AddDeferredBarrier(Func<DateTime> getTime);
        void Run();
        void Run(DateTime at);
    }

    public class Timeline : ITimeline
    {
        #region members

        private readonly List<ActionItem> _actions;

        #endregion

        #region construction

        public static ITimeline New()
        {
            return
                new Timeline();
        }

        private Timeline()
        {
            _actions = new List<ActionItem>();
        }

        #endregion

        #region private

        private void run(DateTime at)
        {
            DateTime tm = at;

            foreach (var item in _actions)
            {
                tm += item.Delay;

                if (item.Action == null)
                    continue;

                item.Action(tm + item.Shift);

                if (tm < item.Barrier)
                    tm = item.Barrier;
            }
        }

        #endregion

        #region ITimeline

        void ITimeline.AddAction(Action<DateTime> action, TimeSpan delay)
        {
            _actions.Add(new ActionItem
                {
                    Action = action,
                    Delay = delay,
                    Shift = TimeSpan.Zero,
                    Barrier = DateTime.MinValue
                });
        }

        void ITimeline.AddActionAt(Action<DateTime> action, TimeSpan shift)
        {
            _actions.Add(new ActionItem
                {
                    Action = action,
                    Delay = TimeSpan.Zero,
                    Shift = shift,
                    Barrier = DateTime.MinValue
                });
        }

        void ITimeline.AddBarrier(DateTime time)
        {
            _actions.Add(new ActionItem
                {
                    Action = null,
                    Delay = TimeSpan.Zero,
                    Shift = TimeSpan.Zero,
                    Barrier = time
                });
        }

        void ITimeline.AddDeferredBarrier(Func<DateTime> getTime)
        {
            var item = new ActionItem
                {
                    Delay = TimeSpan.Zero,
                    Shift = TimeSpan.Zero,
                    Barrier = DateTime.MinValue
                };

            item.Action = 
                tm => item.Barrier = getTime();

            _actions.Add(item);
        }

        void ITimeline.Run()
        {
            run(DateTime.UtcNow);
        }

        void ITimeline.Run(DateTime at)
        {
            run(at);
        }

        #endregion
    }
}