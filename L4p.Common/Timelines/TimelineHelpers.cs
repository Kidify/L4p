using L4p.Common.Timelines.FluentApi;

namespace L4p.Common.Timelines
{
    public static class TimelineHelpers
    {
        public static ITimelineBuilder Setup(this ITimeline timeline)
        {
            return
                TimelineBuilder.New(timeline);
        }
    }
}