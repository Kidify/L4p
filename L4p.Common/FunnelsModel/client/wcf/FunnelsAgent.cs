using System;
using System.ServiceModel;
using L4p.Common.FunnelsModel.comm;

namespace L4p.Common.FunnelsModel.client.wcf
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple,
        IncludeExceptionDetailInFaults = true
    )]
    class FunnelsAgent : comm.IFunnelsAgent
    {
        #region members

        private readonly IFunnelsManagerEx _funnels;

        #endregion

        #region construction

        public static comm.IFunnelsAgent New(IFunnelsManagerEx funnels)
        {
            return
                new FunnelsAgent(funnels);
        }

        private FunnelsAgent(IFunnelsManagerEx funnels)
        {
            _funnels = funnels;
        }

        #endregion

        #region IFunnelsAgent

        void comm.IFunnelsAgent.PruneFunnelData(Guid storeId, string path)
        {
            _funnels.PruneFunnelData(storeId, path);
        }

        void comm.IFunnelsAgent.ShopIsRemoved(ShopInfo shopInfo)
        {
            _funnels.ShopIsRemoved(shopInfo);
        }

        #endregion
    }
}