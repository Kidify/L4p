using System;
using L4p.Common.FunnelsModel.comm;
using L4p.Common.Wcf;

namespace L4p.Common.FunnelsModel.hub.wcf
{
    class FunnelsAgent : WcfProxy<comm.IFunnelsAgent>, comm.IFunnelsAgent
    {
        public static comm.IFunnelsAgent New(string uri) { return new FunnelsAgent(uri); }
        private FunnelsAgent(string uri) : base(uri) {}

        void comm.IFunnelsAgent.PruneFunnelData(Guid storeId, string tag) { Channel.PruneFunnelData(storeId, tag); }
        void comm.IFunnelsAgent.ShopIsRemoved(ShopInfo shopInfo) { Channel.ShopIsRemoved(shopInfo); }
    }
}