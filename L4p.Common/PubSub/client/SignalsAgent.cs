using System;
using System.Diagnostics;
using L4p.Common.Extensions;
using L4p.Common.IoCs;
using L4p.Common.Loggers;
using L4p.Common.PubSub.comm;
using L4p.Common.Wcf;

namespace L4p.Common.PubSub.client
{
    interface ISignalsAgent : comm.ISignalsAgent
    {
        string AgentUri { get; }
        void Start(ISignalsManagerEx signals);
        void Stop();
    }

    class SignalsAgent : ISignalsAgent
    {
        #region members

        private readonly string _agentUri;
        private readonly IWcfHost _host;
        private readonly ILogFile _log;
        private readonly ISignalsConfigRa _configRa;
        private readonly ISignalsManagerEx _signals;

        #endregion

        #region construction

        public static ISignalsAgent New(IIoC ioc)
        {
            return
                new SignalsAgent(ioc);
        }

        private string make_agent_uri(SignalsConfig config)
        {
            var guid = Guid.NewGuid();

            var host = Environment.MachineName;
            var port = config.Port;

            var process = "{0}.{1}".Fmt(
                Process.GetCurrentProcess().ProcessName,
                Process.GetCurrentProcess().Id);

            var uri = config.AgentUri.Substitute(new
                {
                    host, port, process, guid
                });

            return uri;
        }

        private SignalsAgent(IIoC ioc)
        {
            _log = ioc.Resolve<ILogFile>();
            _configRa = ioc.Resolve<ISignalsConfigRa>();
            _signals = ioc.Resolve<ISignalsManagerEx>();

            _agentUri = make_agent_uri(_configRa.Values);

            var target = wcf.SignalsAgent.New(this);
            _host = WcfHost<comm.ISignalsAgent>.NewAsync(_log, target);
        }

        #endregion

        #region private
        #endregion

        #region interface

        public void Dispose()
        {

        }
        
        void comm.ISignalsAgent.HelloFrom(string agent, int snapshotId)
        {
            _signals.GotHelloMsg(this, agent, snapshotId);
        }

        void comm.ISignalsAgent.GoodbyeFrom(string agent)
        {
            _signals.GotGoodbyeMsg(this, agent);
        }

        void comm.ISignalsAgent.Publish(PublishMsg msg)
        {
            _signals.GotPublishMsg(this, msg);
        }

        void comm.ISignalsAgent.ClearAgentsList()
        {
            _signals.ClearAgentsList(this);
        }

        void comm.ISignalsAgent.FilterTopicMsgs(TopicFilterMsg msg)
        {
            _signals.FilterTopicMsgs(this, msg);
        }

        void comm.ISignalsAgent.Heartbeat()
        {
        }

        string ISignalsAgent.AgentUri
        {
            get { return _agentUri; }
        }

        void ISignalsAgent.Start(ISignalsManagerEx signals)
        {
            _host.StartAt(_agentUri);
        }

        void ISignalsAgent.Stop()
        {
            _host.Close();
        }

        #endregion
    }
}