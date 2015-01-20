namespace L4p.Common.DumpToLogs
{
    public interface IAmAComponent
    {
        string Name { get; }
        string Version { get; }
        string LogName { get; }
    }

    class UnknownComponent : IAmAComponent
    {
        string IAmAComponent.Name { get { return "Unknown"; } }
        string IAmAComponent.Version { get { return "0.0.0"; } }
        string IAmAComponent.LogName { get { return "temp.log"; } }
    }
}