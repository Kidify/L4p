using System.ServiceModel;
using L4p.Common.FunnelsModel.comm;

namespace L4p.Common.FunnelsModel.hub.wcf
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple,
        IncludeExceptionDetailInFaults = true
    )]
    public class FunnelsResolver : IFunnelsResolver
    {
        private readonly IFunnelsResolver _impl;

        public static IFunnelsResolver New(IFunnelsResolver impl) { return new FunnelsResolver(impl); }
        private FunnelsResolver(IFunnelsResolver impl) { _impl = impl; }

        void IFunnelsResolver.RegisterShop(ShopInfo info) { _impl.RegisterShop(info); }
        void IFunnelsResolver.ShopIsRemoved(ShopInfo info) { _impl.ShopIsRemoved(info); }
        ShopInfo IFunnelsResolver.ResolveShop(StoreInfo info) { return _impl.ResolveShop(info); }
    }
}