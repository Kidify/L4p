using L4p.Common.DumpToLogs;

namespace L4p.Common.ConfigurationFiles
{
    public class ConfigRaComponent : IAmAComponent
    {
        public static readonly string Name = "config-ra";
        public static readonly string Version = "0.0.1";
        public static readonly string LogName = "bootstrap.log";

        string IAmAComponent.Name { get { return Name; } }
        string IAmAComponent.Version { get { return Version; } }
        string IAmAComponent.LogName { get { return LogName; } }
    }
}