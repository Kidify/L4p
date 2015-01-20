using System;
using L4p.Common.Extensions;
using L4p.Common.FunnelsModel.comm;
using L4p.Common.FunnelsModel.config;
using L4p.Common.IoCs;
using L4p.Common.Loggers;
using L4p.Common.Wcf;

namespace L4p.Common.FunnelsModel.io
{
    interface IIoConnector
    {
        IFunnelsShop ConnectWriter(StoreInfo info);
        IFunnelsShop ConnectReader(StoreInfo info);
        void DisconnectWriter(StoreInfo info, IFunnelsShop shop);
        void DisconnectReader(StoreInfo info, IFunnelsShop shop);
    }

    class IoConnector : IIoConnector
    {
        #region members

        private readonly ILogFile _log;
        private readonly IFmConfigRa _config;

        #endregion

        #region construction

        public static IIoConnector New(IIoC ioc)
        {
            return
                new IoConnector(ioc);
        }

        private IoConnector(IIoC ioc)
        {
            _log = ioc.Resolve<ILogFile>();
            _config = ioc.Resolve<IFmConfigRa>();
        }

        #endregion

        #region private

        private ShopInfo resolve_funnels_shop(FunnelsConfig config, StoreInfo info)
        {
            var host = config.ResolvingHost;
            var port = config.Port;

            var resolvingUri = config.ResolvingAt.Fmt(host, port);
            var resolver = client.wcf.FunnelsResolver.New(resolvingUri);

            using (resolver as IDisposable)
            {
                var shopInfo = resolver.ResolveShop(info);
                return shopInfo;
            }
        }

        private void disconnect_shop(StoreInfo info, IFunnelsShop shop)
        {
            if (shop == null)
                return;

            try
            {
                shop.StoreIsRemoved(info.StoreId);
                _log.Trace("Funnel '{0}' is disconnected from '{1}'", info.FunnelId, info.ShopUri);
            }
            catch
            { }

            try
            {
                var wcf = (IWcfProxy)shop;
                wcf.Close();
            }
            catch
            { }
        }
        
        #endregion

        #region interface

        IFunnelsShop IIoConnector.ConnectWriter(StoreInfo info)
        {
            var config = _config.Config;

            var shopInfo = resolve_funnels_shop(config, info);
            _log.Trace("Funnel '{0}' is resolved to '{1}'", info.FunnelId, shopInfo.Uri);

            info.ShopId = shopInfo.ShopId;
            info.ShopUri = shopInfo.Uri;

            var shop = client.wcf.FunnelsShopAsyncWriter.New(info.ShopUri);
            _log.Trace("Funnel '{0}' is connected to '{1}'", info.FunnelId, info.ShopUri);

            shop.RegisterStore(info);

            return shop;
        }

        IFunnelsShop IIoConnector.ConnectReader(StoreInfo info)
        {
            var config = _config.Config;

            var shopInfo = resolve_funnels_shop(config, info);
            _log.Trace("Funnel '{0}' writer is resolved to '{1}'", info.FunnelId, shopInfo.Uri);

            info.ShopId = shopInfo.ShopId;
            info.ShopUri = shopInfo.Uri;

            var shop = client.wcf.FunnelsShop.New(info.ShopUri);
            _log.Trace("Funnel '{0}' reader is connected to '{1}'", info.FunnelId, info.ShopUri);

            shop.RegisterStore(info);

            return shop;
        }

        void IIoConnector.DisconnectWriter(StoreInfo info, IFunnelsShop shop)
        {
            disconnect_shop(info, shop);
        }

        void IIoConnector.DisconnectReader(StoreInfo info, IFunnelsShop shop)
        {
            disconnect_shop(info, shop);
        }

        #endregion
    }
}