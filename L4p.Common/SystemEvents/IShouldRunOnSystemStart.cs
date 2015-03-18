using L4p.Common.Loggers;

namespace L4p.Common.SystemEvents
{
    public interface IShouldRunOnSystemStart
    {
        int InitializationOrder { get; }
        void SystemIsBeingStarted(string moduleKey, ILogFile log);
    }
}