namespace L4p.Common.DumpToLogs
{
    public class DumpComponent : IAmAComponent
    {
        public static readonly string Name = "dump-manager";
        public static readonly string Version = "1.0.1";
        public static readonly string LogName = "bootstrap.log";
        public static readonly string ConfigName = "dumps.js";

        string IAmAComponent.Name { get { return Name; } }
        string IAmAComponent.Version { get { return Version; } }
        string IAmAComponent.LogName { get { return LogName; } }
    }
}