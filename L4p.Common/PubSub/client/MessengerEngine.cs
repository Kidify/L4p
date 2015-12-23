using System;
using System.Collections.Generic;
using System.Dynamic;
using System.ServiceModel;
using System.Threading;
using L4p.Common.DumpToLogs;
using L4p.Common.Extensions;
using L4p.Common.Helpers;
using L4p.Common.IoCs;
using L4p.Common.Json;
using L4p.Common.Loggers;
using L4p.Common.PubSub.client.Io;

namespace L4p.Common.PubSub.client
{
    interface IMessengerEngine : IHaveDump
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

    class MessengerEngine : IMessengerEngine
    {
        #region counters

        class Counters
        {
            public long HelloMsgSent;
            public long GoodbyeMsgSent;
            public long PublishMsgSent;
            public long FilterTopicMsgSent;
            public long HeartbeatMsgSent;
            public long HubMsgSent;
            public long HubMsgFailed;
            public long IoFailed;
            public long MsgNotSent;
            public long IoRetry;
            public long PublishMsgIoFailed;
            public long FilterInfoIoMsgFailed;
            public long TopicFilterMsgIoFailed;
            public long HeartbeatMsgIoFailed;
            public long UnknownMsgIoFailed;
            public long EndpointNotFound;
        }

        #endregion

        #region members

        private static readonly comm.HeartbeatMsg _heartbeatMsg = new comm.HeartbeatMsg();

        private readonly Dictionary<Type, Action<comm.IoMsg>> _msg2failureHandler;

        private readonly Counters _counters;
        private readonly string _hubUri;
        private readonly ILogFile _log;
        private readonly ISignalsConfigRa _configRa;
        private readonly IAgentConnector _connector;
        private readonly IAgentsRepo _agents;
        private readonly SignalsConfig _cachedConfig;

        #endregion

        #region construction

        public static IMessengerEngine New(IIoC ioc)
        {
            return
                new MessengerEngine(ioc);
        }

        private MessengerEngine(IIoC ioc)
        {
            _msg2failureHandler = new Dictionary<Type, Action<comm.IoMsg>> {
                {typeof(comm.PublishMsg), msg => update_failure_counters((comm.PublishMsg) msg)},
                {typeof(comm.FilterInfo), msg => update_failure_counters((comm.FilterInfo) msg)},
                {typeof(comm.TopicFilterMsg), msg => update_failure_counters((comm.TopicFilterMsg) msg)},
                {typeof(comm.HeartbeatMsg), msg => update_failure_counters((comm.HeartbeatMsg) msg)}
            };

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

        private void update_failure_counters(comm.PublishMsg msg)
        {
            Interlocked.Increment(ref _counters.PublishMsgIoFailed);

            var topic = msg.Topic;
            Interlocked.Increment(ref topic.Details.Counters.IoFailed);

            if (msg.RetryCount == 0)
                Interlocked.Increment(ref topic.Details.Counters.MsgNotSent);
        }

        private void update_failure_counters(comm.FilterInfo msg)
        {
            Interlocked.Increment(ref _counters.FilterInfoIoMsgFailed);
        }

        private void update_failure_counters(comm.TopicFilterMsg msg)
        {
            Interlocked.Increment(ref _counters.TopicFilterMsgIoFailed);
        }

        private void update_failure_counters(comm.HeartbeatMsg msg)
        {
            Interlocked.Increment(ref _counters.HeartbeatMsgIoFailed);
        }

        private void update_failure_counters(comm.IoMsg msg)
        {
            Action<comm.IoMsg> handler;

            if (!_msg2failureHandler.TryGetValue(msg.GetType(), out handler))
            {
                Interlocked.Increment(ref _counters.UnknownMsgIoFailed);
                return;
            }

            try
            {
                handler(msg);
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Failed to handle failed message");
            }
        }

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
            catch (EndpointNotFoundException)
            {
                Interlocked.Increment(ref _counters.EndpointNotFound);
                Interlocked.Increment(ref _counters.HubMsgFailed);
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

            Interlocked.Increment(ref _counters.MsgNotSent);
        }

        #endregion

        #region interface

        void IMessengerEngine.SendHelloMsg(comm.HelloMsg msg)
        {
            using (var hub = create_hub_proxy(_hubUri))
            {
                send_message(_hubUri, "client.hello",
                    () => hub.Hello(msg));
            }

            Interlocked.Increment(ref _counters.HelloMsgSent);
        }

        void IMessengerEngine.SendGoodbyeMsg(string agentUri)
        {
            using (var hub = create_hub_proxy(_hubUri))
            {
                send_message(_hubUri, "client.goodbye",
                    () => hub.Goodbye(agentUri));
            }

            Interlocked.Increment(ref _counters.GoodbyeMsgSent);
        }

        void IMessengerEngine.SendPublishMsg(string agentUri, comm.PublishMsg msg)
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

        void IMessengerEngine.FilterTopicMsgs(string agentUri, comm.TopicFilterMsg msg)
        {
            var proxy = get_agent_proxy(agentUri);
            proxy.FilterTopicMsgs(msg);

            Interlocked.Increment(ref _counters.FilterTopicMsgSent);
        }

        void IMessengerEngine.AgentIsHere(string agentUri)
        {
            var proxy = get_agent_proxy(agentUri);

            proxy.Heartbeat(_heartbeatMsg);
            Interlocked.Increment(ref _counters.HeartbeatMsgSent);
        }

        void IMessengerEngine.AgentIsGone(string agentUri)
        {
            var proxy = _agents.RemoveAgent(agentUri);

            if (proxy == null)
                return;

            _connector.DisconnectAgent(proxy);
        }

        void IMessengerEngine.IoFailed(IAgentWriter proxy, comm.IoMsg msg, Exception ex, string at)
        {
            Interlocked.Increment(ref _counters.IoFailed);
            update_failure_counters(msg);

            var msgType = msg.GetType().Name;

            _log.Error("Msg ({0}): io error in '{1}' of '{2}' (retryCount={3}) to agent '{4}' (agent will be reconnected); {5}", 
                msg.Guid.AsStr(), at, msgType, msg.RetryCount, msg.AgentUri, ex.Message);

            var removedAgent = _agents.RemoveAgent(proxy);

            if (removedAgent == null)
                return;

            _connector.DisconnectAgent(removedAgent);

            retry_io(msg);
        }

        void IMessengerEngine.Idle()
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