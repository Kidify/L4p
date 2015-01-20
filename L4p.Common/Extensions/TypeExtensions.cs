using System;
using System.Diagnostics;

namespace L4p.Common.Extensions
{
    public static class TypeExtensions
    {
        public static string AsServiceName(this Type type)
        {
            var typeName = type.Name;
            var machine = Environment.MachineName.ToLowerInvariant();

            var process = Process.GetCurrentProcess();
            var processName = process.ProcessName;
            var processId = process.Id;

            var serviceName = "{0}.{1}.{2}@{3}".Fmt(typeName, processName, processId, machine);

            return serviceName;
        }
    }
}