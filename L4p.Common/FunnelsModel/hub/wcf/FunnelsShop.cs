using System;
using System.ServiceModel;
using L4p.Common.FunnelsModel.comm;

namespace L4p.Common.FunnelsModel.hub.wcf
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple,
        IncludeExceptionDetailInFaults = true
    )]
    public class FunnelsShop : IFunnelsShop
    {
        private readonly IFunnelsShop _impl;

        public static IFunnelsShop New(IFunnelsShop impl) { return new FunnelsShop(impl); }
        private FunnelsShop(IFunnelsShop impl) { _impl = impl; }

        ShopInfo IFunnelsShop.GetInfo() { return _impl.GetInfo(); }
        ShopInfo IFunnelsShop.RegisterStore(StoreInfo info) { return _impl.RegisterStore(info); }
        void IFunnelsShop.StoreIsRemoved(Guid storeId) { _impl.StoreIsRemoved(storeId); }
        void IFunnelsShop.PublishPost(Guid storeId, Post post) { _impl.PublishPost(storeId, post); }
        Post IFunnelsShop.GetPost(Guid storeId, string path) { return _impl.GetPost(storeId, path); }
        void IFunnelsShop.KeepAlive() { _impl.KeepAlive(); }
    }
}