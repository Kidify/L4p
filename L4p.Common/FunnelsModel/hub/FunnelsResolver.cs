using System;
using System.Diagnostics;
using System.ServiceModel;
using L4p.Common.FunnelsModel.comm;
using L4p.Common.IoCs;
using L4p.Common.Loggers;

namespace L4p.Common.FunnelsModel.hub
{
    public class FunnelsResolver : IFunnelsResolver
    {
        #region members

        private readonly ILogFile _log;

        private ShopInfo _info;

        #endregion

        #region construction

        public static IFunnelsResolver New(IIoC ioc)
        {
            return
                new FunnelsResolver(ioc);
        }

        private FunnelsResolver(IIoC ioc)
        {
            _log = ioc.Resolve<ILogFile>();
        }

        #endregion

        #region private
        #endregion

        #region IFunnelsHubHost

        #endregion

        void IFunnelsResolver.RegisterShop(ShopInfo info)
        {
            _info = info;
            _log.Info("Resolver: shop at {0} is registered", info.Uri);
        }

        void IFunnelsResolver.ShopIsRemoved(ShopInfo info)
        {
        }

        ShopInfo IFunnelsResolver.ResolveShop(StoreInfo info)
        {
            _log.Info("Resolver: funnel '{0}' is resolved to '{1}'", info.FunnelId, info.AgentUri);
            return _info;
        }
    }
}