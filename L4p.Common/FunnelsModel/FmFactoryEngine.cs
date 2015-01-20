using System;
using L4p.Common.FunnelsModel.client;
using L4p.Common.FunnelsModel.comm;
using L4p.Common.FunnelsModel.io;
using L4p.Common.IoCs;
using L4p.Common.Loggers;
using IFunnelsAgent = L4p.Common.FunnelsModel.client.IFunnelsAgent;

namespace L4p.Common.FunnelsModel
{
    interface IFmFactoryEngine
    {
        IFunnelStore MakeStore(string funnelId);
    }

    class FmFactoryEngine : IFmFactoryEngine
    {
        #region members

        private readonly IIoC _ioc;
        private readonly ILogFile _log;
        private readonly IIoSink _sink;
        private readonly IFunnelsAgent _agent;

        #endregion

        #region construction

        public static IFmFactoryEngine New(IIoC ioc)
        {
            return
                new FmFactoryEngine(ioc);
        }

        private FmFactoryEngine(IIoC ioc)
        {
            _ioc = ioc;
            _log = ioc.Resolve<ILogFile>();
            _sink = ioc.Resolve<IIoSink>();
            _agent = ioc.Resolve<IFunnelsAgent>();
        }

        #endregion

        #region private

        private IFunnelStore make_funnel_store(string funnelId)
        {
            var storeId = Guid.NewGuid();
            var agentInfo = _agent.Info;

            var info = new StoreInfo
                {
                    AgentId = agentInfo.AgentId,
                    AgentUri = agentInfo.Uri,
                    FunnelId = funnelId,
                    StoreId = storeId
                };

            var shop = FunnelsShop.New(info, _ioc);
            var store = FunnelStore.New(storeId, shop, _log);

            store.FunnelId = funnelId;
            store.DeadAt = DateTime.MaxValue;

            return store;
        }

        #endregion

        #region interface

        IFunnelStore IFmFactoryEngine.MakeStore(string funnelId)
        {
            try
            {
                return
                    make_funnel_store(funnelId);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Failed to make store for funnel '{0}'", funnelId);
                return Null.Store;
            }
        }

        #endregion
    }
}