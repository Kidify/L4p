using System;

namespace L4p.Common.FunnelsModel
{
    public interface IFunnelTimeline
    {
        T GetAt<T>(DateTime at);
        void StoreAt<T>(T data, DateTime at);
    }
}