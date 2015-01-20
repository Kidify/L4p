using System;

namespace L4p.Common.FunnelsModel
{
    public interface IFunnelTags
    {
        void Add(int tag);
        void Add(uint tag);
        void Add(long tag);
        void Add(string tag);
        void Add(Guid tag);
    }
}