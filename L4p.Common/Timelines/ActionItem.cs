using System;

namespace L4p.Common.Timelines
{
    class ActionItem
    {
        public Action<DateTime> Action { get; set; }
        public TimeSpan Delay { get; set; }
        public TimeSpan Shift { get; set; }
        public DateTime Barrier { get; set; }
    }
}