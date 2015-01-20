using System;
using L4p.Common.FunnelsModel.comm;
using L4p.Common.IoCs;
using L4p.Common.Loggers;

namespace L4p.Common.FunnelsModel.io
{
    interface IIoReader
    {
        Post GetPost(Guid storeId, string path);
        void CloseConnection();
    }

    class IoReader : IIoReader
    {
        #region members

        private readonly ILogFile _log;

        private IFunnelsShop _shop;
        private readonly IIoConnector _io;
        private readonly StoreInfo _info;

        #endregion

        #region construction

        public static IIoReader New(IIoC ioc, StoreInfo info)
        {
            return
                new IoReader(ioc, info);
        }

        private IoReader(IIoC ioc, StoreInfo info)
        {
            _log = ioc.Resolve<ILogFile>();
            _io = ioc.Resolve<IIoConnector>();
            _info = info;
        }

        #endregion

        #region private

        private IFunnelsShop get_connection(StoreInfo info)
        {
            if (_shop != null)
                return _shop;

            _shop = _io.ConnectReader(info);
            return _shop;
        }

        private void close_connection(StoreInfo info)
        {
            if (_shop == null)
                return;

            var shop = _shop;
            _shop = null;

            _io.DisconnectReader(info, shop);
        }

        #endregion

        #region interface

        Post IIoReader.GetPost(Guid storeId, string path)
        {
            var shop = get_connection(_info);

            try
            {
                var post = shop.GetPost(storeId, path);
                return post;
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Failed to read data from funnel '{0}'", _info.FunnelId);
                close_connection(_info);
            }

            return null;
        }

        void IIoReader.CloseConnection()
        {
            close_connection(_info);
        }

        #endregion
    }
}