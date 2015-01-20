using L4p.Common.Helpers;
using L4p.Common.Loggers;

namespace L4p.Common.ActionQueues
{
    public static class ActionQueueHelpers
    {
        public static void Run(this IActionQueue que, ILogFile log = null)
        {
            log = log ?? LogFile.Null;

            while (true)
            {
                var action = que.Pop();

                if (action == null)
                    break;

                Try.Catch.Handle(action,
                    ex => log.Error(ex));
            }
        }
    }
}