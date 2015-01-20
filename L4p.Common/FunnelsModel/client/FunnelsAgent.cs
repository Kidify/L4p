using System;
using System.Net;
using System.Net.Sockets;
using L4p.Common.FunnelsModel.comm;
using L4p.Common.FunnelsModel.config;
using L4p.Common.IoCs;
using L4p.Common.Loggers;
using L4p.Common.Wcf;

namespace L4p.Common.FunnelsModel.client
{
    interface IFunnelsAgent : IShouldBeStarted
    {
        AgentInfo Info { get; }

        void Start();
        void Stop();
    }

    class FunnelsAgent : IFunnelsAgent
    {
        #region members

        private readonly ILogFile _log;
        private readonly IFmConfigRa _config;
        private readonly AgentInfo _info;
        private readonly IWcfHost _host;

        #endregion

        #region construction

        public static IFunnelsAgent New(IIoC ioc)
        {
            return 
                new FunnelsAgent(ioc);
        }

        private FunnelsAgent(IIoC ioc)
        {
            _log = ioc.Resolve<ILogFile>();
            _config = ioc.Resolve<IFmConfigRa>();

            var agentId = Guid.NewGuid();

            int port = find_free_port();
            var uri = _config.MakeAgentUri(agentId, port);

            _info = new AgentInfo {AgentId = agentId, Uri = uri};

            var funnels = ioc.Resolve<IFunnelsManagerEx>();

            var target = wcf.FunnelsAgent.New(funnels);
            _host = WcfHost<comm.IFunnelsAgent>.NewAsync(_log, target);
        }

        #endregion

        #region private

        private static int find_free_port()
        {
            var endPoint = new IPEndPoint(IPAddress.Any, 0);

            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socket.Bind(endPoint);
                var local = (IPEndPoint)socket.LocalEndPoint;
                return local.Port;
            }
        }

        #endregion

        #region interface

        AgentInfo IFunnelsAgent.Info
        {
            get { return _info; }
        }

        void IFunnelsAgent.Start()
        {
            _host.StartAt(_info.Uri);
        }

        void IFunnelsAgent.Stop()
        {
            _host.Close();
        }

        #endregion
    }
}