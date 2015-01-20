using L4p.Common.DumpToLogs;

namespace L4p.Common.PubSub
{
    public class SignalsComponent : IAmAComponent
    {
        public static readonly string Name = "signals";
        public static readonly string Version = "1.1.3.async-push";
        public static readonly string LogName = "signals.log";

        string IAmAComponent.Name { get { return Name; } }
        string IAmAComponent.Version { get { return Version; } }
        string IAmAComponent.LogName { get { return LogName; } }
    }
}