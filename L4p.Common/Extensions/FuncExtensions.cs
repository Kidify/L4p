using System;
using System.Threading;

namespace L4p.Common.Extensions
{
    public static class FuncExtensions
    {
        public static T WithRetries<T>(this Func<T> func, int count)
        {
            TimeSpan nextRetry = 10.Milliseconds();

            do
            {
                try
                {
                    return
                        func();
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