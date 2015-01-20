using System;
using System.Threading;

namespace L4p.Common.Extensions
{
    public static class ActionExtensions
    {
        public static void WithRetries(this Action action, int count)
        {
            TimeSpan nextRetry = 10.Milliseconds();

            do
            {
                try
                {
                    action();
                    return;
                }
                catch
                {
                    if (count-- == 0)
                        throw;

                    nextRetry = nextRetry + nextRetry;
                    Thread.Sleep(nextRetry);
                }
            }
            while (true);
        }
    }
}