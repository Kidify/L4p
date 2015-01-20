using System;
using L4p.Common.ConfigurationFiles;
using L4p.Common.Extensions;

namespace L4p.Common.FunnelsModel.config
{
    interface IFmConfigRa
    {
        FunnelsConfig Config { get; }
        string MakeResolvingUri();
        string MakeAgentUri(Guid agentId, int port);
    }

    class FmConfigRa : IFmConfigRa
    {
        #region members

        private readonly FunnelsConfig _config;

        #endregion

        #region construction

        public static IFmConfigRa New(FunnelsConfig config)
        {
            return
                new FmConfigRa(config);
        }

        private FmConfigRa(FunnelsConfig config)
        {
            _config = config;
        }

        #endregion

        #region private
        #endregion

        #region interface

        FunnelsConfig IFmConfigRa.Config
        {
            get { return _config; }
        }

        string IFmConfigRa.MakeResolvingUri()
        {
            return
                _config.ResolvingAt.Fmt(_config.ResolvingHost, _config.Port);
        }

        string IFmConfigRa.MakeAgentUri(Guid agentId, int port)
        {
            return
                _config.AgentUri.Fmt(_config.ResolvingHost, port, agentId);
        }

        #endregion
    }
}