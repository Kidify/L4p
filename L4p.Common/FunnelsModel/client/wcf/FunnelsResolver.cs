using L4p.Common.FunnelsModel.comm;
using L4p.Common.Wcf;

namespace L4p.Common.FunnelsModel.client.wcf
{
    class FunnelsResolver : WcfProxy<IFunnelsResolver>, IFunnelsResolver
    {
        public static IFunnelsResolver New(string uri) { return new FunnelsResolver(uri); }
        private FunnelsResolver(string uri) : base(uri) { }

        void IFunnelsResolver.RegisterShop(ShopInfo info) { Channel.RegisterShop(info); }
        void IFunnelsResolver.ShopIsRemoved(ShopInfo info) { Channel.ShopIsRemoved(info); }
        ShopInfo IFunnelsResolver.ResolveShop(StoreInfo info) { return Channel.ResolveShop(info); }
    }
}