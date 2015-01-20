using System;
using L4p.Common.FunnelsModel.comm;

namespace L4p.Common.FunnelsModel.hub
{
    interface IFunnelsAgent
    {
        Guid AgentId { get; }
        void PruneFunnelData(Guid storeId, string path);
    }

    class FunnelsAgent : IFunnelsAgent
    {
        #region members

        private readonly AgentInfo _info;
        private readonly comm.IFunnelsAgent _proxy;

        #endregion

        #region construction

        public static IFunnelsAgent New(AgentInfo info)
        {
            return
                new FunnelsAgent(info);
        }

        private FunnelsAgent(AgentInfo info)
        {
            _info = info;
            _proxy = wcf.FunnelsAgent.New(info.Uri);
        }

        #endregion

        #region private
        #endregion

        #region IFunnelsAgent

        Guid IFunnelsAgent.AgentId
        {
            get { return _info.AgentId; }
        }

        void IFunnelsAgent.PruneFunnelData(Guid storeId, string path)
        {
            _proxy.PruneFunnelData(storeId, path);
        }

        #endregion
    }
}