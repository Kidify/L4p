using System.Dynamic;

namespace L4p.Common.DumpToLogs
{
    public interface IHaveDump
    {
        ExpandoObject Dump(dynamic root = null);
    }
}