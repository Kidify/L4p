using System;
using L4p.Common.FunnelsModel.comm;
using L4p.Common.IoCs;
using L4p.Common.Loggers;

namespace L4p.Common.FunnelsModel.io
{
    interface IIoWriter
    {
        void Send(Ioop ioop);
        void CloseConnection();
    }

    class IoWriter : IIoWriter
    {
        #region members

        private readonly ILogFile _log;
        private readonly IIoConnector _io;
        private readonly StoreInfo _info;

        private IFunnelsShop _shop;

        #endregion

        #region construction

        public static IIoWriter New(IIoC ioc, StoreInfo info)
        {
            return
                new IoWriter(ioc, info);
        }

        private IoWriter(IIoC ioc, StoreInfo info)
        {
            _log = ioc.Resolve<ILogFile>();
            _io = ioc.Resolve<IIoConnector>();
            _info = info;
        }

        #endregion

        #region private

        private void ensure_connection(StoreInfo info)
        {
            if (_shop != null)
                return;

            _shop = _io.ConnectWriter(info);
        }

        private void close_connection(StoreInfo info)
        {
            if (_shop == null)
                return;

            var shop = _shop;
            _shop = null;

            _io.DisconnectWriter(info, shop);
        }

        #endregion

        #region interface

        void IIoWriter.Send(Ioop ioop)
        {
            ensure_connection(_info);

            try
            {
                ioop.Io(_shop);
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Failed to send io {0}; funnelId='{1}'", ioop.SequenceId, _info.FunnelId);
                close_connection(_info);
            }
        }

        void IIoWriter.CloseConnection()
        {
            close_connection(_info);
        }

        #endregion
    }
}