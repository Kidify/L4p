using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading;
using L4p.Common.DumpToLogs;
using L4p.Common.IoCs;
using L4p.Common.Json;
using L4p.Common.Loggers;
using L4p.Common.PubSub.client.Io;

namespace L4p.Common.PubSub.client
{
    interface IMessangerEngine : IHaveDump
    {
        void SendHelloMsg(comm.HelloMsg msg);
        void SendGoodbyeMsg(string agentUri);
        void SendPublishMsg(string agentUri, comm.PublishMsg msg);
        void FilterTopicMsgs(string agentUri, comm.TopicFilterMsg msg);
        void AgentIsHere(string agentUri);
        void AgentIsGone(string agentUri);
        void IoFailed(IAgentWriter proxy, comm.IoMsg msg, Exception ex, string at);
        void Idle();
    }

    class MessangerEngine : IMessangerEngine
    {
        #region counters

        class Counters
        {
            public int HelloMsgSent;
            public int GoodbyeMsgSent;
            public int PublishMsgSent;
            public int FilterTopicMsgSent;
            public int HeartbeatMsgSent;
            public int HubMsgSent;
            public int HubMsgFailed;
            public int MsgIoFailed;
            public int PermanentIoFailures;
            public int IoRetry;
        }

        #endregion

        #region members

        private static readonly comm.HeartbeatMsg _heartbeatMsg = new comm.HeartbeatMsg();

        private readonly Counters _counters;
        private readonly string _hubUri;
        private readonly ILogFile _log;
        private readonly ISignalsConfigRa _configRa;
        private readonly IAgentConnector _connector;
        private readonly IAgentsRepo _agents;
        private readonly SignalsConfig _cachedConfig;

        #endregion

        #region construction

        public static IMessangerEngine New(IIoC ioc)
        {
            return
                new MessangerEngine(ioc);
        }

        private MessangerEngine(IIoC ioc)
        {
            _counters = new Counters();
            _configRa = ioc.Resolve<ISignalsConfigRa>();

            var config = _configRa.Values;
            _cachedConfig = config;
            _log = ThrottledLog.NewSync(config.ThrottledLogTtl, ioc.Resolve<ILogFile>());

            _hubUri = _configRa.MakeHubUri();
            _connector = ioc.Resolve<IAgentConnector>();
            _agents = AgentsRepo.NewSync();
        }

        #endregion

        #region private

        private void set_agent_proxy(string uri, IAgentWriter proxy)
        {
            var prev = _agents.SetAgentProxy(uri, proxy);

            if (prev == null)
                return;

            _connector.DisconnectAgent(prev);
        }

        private IAgentWriter get_agent_proxy(string agentUri)
        {
            var proxy = _agents.GetAgentProxy(agentUri);

            if (proxy != null)
                return proxy;

            proxy = _connector.ConnectAgent(agentUri, this);
            set_agent_proxy(agentUri, proxy);

            return proxy;
        }

        private comm.ISignalsHub create_hub_proxy(string uri)
        {
            var proxy = wcf.SignalsHub.New(uri);
            return proxy;
        }

        private void send_message(string agentUri, string tag, Action action)
        {
            try
            {
                action();
                Interlocked.Increment(ref _counters.HubMsgSent);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Failure while sending message '{0}' to '{1}'", tag, agentUri);
                Interlocked.Increment(ref _counters.HubMsgFailed);
            }
        }

        private void retry_io(comm.IoMsg msg)
        {
            var retryIt = msg.Retry;

            for (;;)    // do only once
            {
                if (retryIt == null)
                    break;

                if (msg.RetryCount == 0)
                    break;

                Interlocked.Increment(ref _counters.IoRetry);

                msg.RetryCount--;
                retryIt();

                return;
            }

            Interlocked.Increment(ref _counters.PermanentIoFailures);
        }

        #endregion

        #region interface

        void IMessangerEngine.SendHelloMsg(comm.HelloMsg msg)
        {
            using (var hub = create_hub_proxy(_hubUri))
            {
                send_message(_hubUri, "client.hello",
                    () => hub.Hello(msg));
            }

            Interlocked.Increment(ref _counters.HelloMsgSent);
        }

        void IMessangerEngine.SendGoodbyeMsg(string agentUri)
        {
            using (var hub = create_hub_proxy(_hubUri))
            {
                send_message(_hubUri, "client.goodbye",
                    () => hub.Goodbye(agentUri));
            }

            Interlocked.Increment(ref _counters.GoodbyeMsgSent);
        }

        void IMessangerEngine.SendPublishMsg(string agentUri, comm.PublishMsg msg)
        {
            Action sendIt = () => {
                var proxy = get_agent_proxy(agentUri);
                proxy.Publish(msg);
            };

            msg.Retry = sendIt;
            msg.RetryCount = _cachedConfig.Client.PublishRetryCount;

            sendIt();

            Interlocked.Increment(ref _counters.PublishMsgSent);
        }

        void IMessangerEngine.FilterTopicMsgs(string agentUri, comm.TopicFilterMsg msg)
        {
            var proxy = get_agent_proxy(agentUri);
            proxy.FilterTopicMsgs(msg);

            Interlocked.Increment(ref _counters.FilterTopicMsgSent);
        }

        void IMessangerEngine.AgentIsHere(string agentUri)
        {
            var proxy = get_agent_proxy(agentUri);

            proxy.Heartbeat(_heartbeatMsg);
            Interlocked.Increment(ref _counters.HeartbeatMsgSent);
        }

        void IMessangerEngine.AgentIsGone(string agentUri)
        {
            var proxy = _agents.RemoveAgent(agentUri);

            if (proxy == null)
                return;

            _connector.DisconnectAgent(proxy);
        }

        void IMessangerEngine.IoFailed(IAgentWriter proxy, comm.IoMsg msg, Exception ex, string at)
        {
            Interlocked.Increment(ref _counters.MsgIoFailed);

            var msgType = msg.GetType().Name;

            _log.Error(ex, "Io error in '{3}' (retryCount={4}) sending '{0}' message to '{1}' (agent will be reconnected); {2}", 
                msg.AgentUri, msgType, msg.ToJson(), at, msg.RetryCount);

            var removedAgent = _agents.RemoveAgent(proxy);

            if (removedAgent == null)
                return;

            _connector.DisconnectAgent(removedAgent);

            retry_io(msg);
        }

        void IMessangerEngine.Idle()
        {
            var config = _configRa.Values;
            var proxies = _agents.GetAll();

            foreach (var proxy in proxies)
            {
                if (proxy.NonActiveSpan < config.Client.HeartbeatSpan)
                    continue;

                proxy.Heartbeat(_heartbeatMsg);
                Interlocked.Increment(ref _counters.HeartbeatMsgSent);
            }
        }

        ExpandoObject IHaveDump.Dump(dynamic root)
        {
            if (root == null)
                root = new ExpandoObject();

            root.HubUri = _hubUri;
            root.Counters = _counters;

            var agents = _agents.GetAll();
            var list = new List<object>();

            foreach (var proxy in agents)
            {
                list.Add(proxy.Dump());
            }

            root.Agents = list.ToArray();

            return root;
        }

        #endregion
    }
}