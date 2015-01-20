using System;
using System.Diagnostics;

namespace L4p.Common.Extensions
{
    public static class TimeSpanExtensions
    {
        #region int

        [DebuggerStepThrough]
        public static TimeSpan Milliseconds(this int milliseconds)
        {
            return
                TimeSpan.FromMilliseconds(milliseconds);
        }

        [DebuggerStepThrough]
        public static TimeSpan Seconds(this int seconds)
        {
            return
                TimeSpan.FromSeconds(seconds);
        }

        [DebuggerStepThrough]
        public static TimeSpan Minutes(this int minutes)
        {
            return
                TimeSpan.FromMinutes(minutes);
        }

        [DebuggerStepThrough]
        public static TimeSpan Hours(this int hours)
        {
            return
                TimeSpan.FromHours(hours);
        }

        [DebuggerStepThrough]
        public static TimeSpan Days(this int days)
        {
            return
                TimeSpan.FromDays(days);
        }

        #endregion

        #region double

        [DebuggerStepThrough]
        public static TimeSpan Milliseconds(this double milliseconds)
        {
            return
                TimeSpan.FromMilliseconds(milliseconds);
        }

        [DebuggerStepThrough]
        public static TimeSpan Seconds(this double seconds)
        {
            return
                TimeSpan.FromSeconds(seconds);
        }

        [DebuggerStepThrough]
        public static TimeSpan Minutes(this double minutes)
        {
            return
                TimeSpan.FromMinutes(minutes);
        }

        [DebuggerStepThrough]
        public static TimeSpan Hours(this double hours)
        {
            return
                TimeSpan.FromHours(hours);
        }

        [DebuggerStepThrough]
        public static TimeSpan Days(this double days)
        {
            return
                TimeSpan.FromDays(days);
        }

        #endregion

        #region decimal

        [DebuggerStepThrough]
        public static TimeSpan Milliseconds(this decimal milliseconds)
        {
            return
                TimeSpan.FromMilliseconds((double) milliseconds);
        }

        [DebuggerStepThrough]
        public static TimeSpan Seconds(this decimal seconds)
        {
            return
                TimeSpan.FromSeconds((double) seconds);
        }

        [DebuggerStepThrough]
        public static TimeSpan Minutes(this decimal minutes)
        {
            return
                TimeSpan.FromMinutes((double) minutes);
        }

        [DebuggerStepThrough]
        public static TimeSpan Hours(this decimal hours)
        {
            return
                TimeSpan.FromHours((double) hours);
        }

        [DebuggerStepThrough]
        public static TimeSpan Days(this decimal days)
        {
            return
                TimeSpan.FromDays((double) days);
        }

        #endregion
    }
}