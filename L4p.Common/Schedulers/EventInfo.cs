using System;

namespace L4p.Common.Schedulers
{
    public class EventInfo
    {
        public DateTime NextShot { get; set; }
        public TimeSpan RepeatAfter { get; set; }
        public int Count { get; set; }

        internal Action Action { get; set; }
        internal int Invoked { get; set; }
        internal int Failed { get; set; }
        internal Exception LastFailure { get; set; }
    }
}