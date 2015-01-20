using System;

namespace L4p.Common.FunnelsModel
{
    public interface IFunnelLogger
    {
        dynamic Error { get; }
        dynamic Warn { get; }
        dynamic Info { get; }
        dynamic Trace { get; }
    }
}