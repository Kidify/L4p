using System;
using System.Dynamic;
using L4p.Common.DumpToLogs;
using L4p.Common.Loggers;

namespace L4p.Common.PubSub.hub
{
    interface IMessengerEngine : IHaveDump
    {
        bool SendHelloMsg(string agentUri, string helloAgentUri, int snapshotId);
        bool SendGoodbyeMsg(string agentUri, string goodbyeAgentUri);
        bool ClearAgentsList(string agentUri);
        void FilterTopicMsgs(string agentUri, comm.TopicFilterMsg msg);
    }

    class MessengerEngine : IMessengerEngine
    {
        #region members

        private readonly ILogFile _log;

        #endregion

        #region construction

        public static IMessengerEngine New(ILogFile log)
        {
            return
                new MessengerEngine(log);
        }

        private MessengerEngine(ILogFile log)
        {
            _log = log;
        }

        #endregion

        #region private

        private wcf.ISignalsAgent create_client_proxy(string uri)
        {
            var proxy = wcf.SignalsAgent.New(uri);
            return proxy;
        }

        private bool send_message(string uri, string tag, Action action)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Failure while sending message '{0}' to '{1}'", tag, uri);
                return false;
            }
        }

        #endregion

        #region interface

        bool IMessengerEngine.SendHelloMsg(string agentUri, string helloAgentUri, int snapshotId)
        {
            using (var client = create_client_proxy(agentUri))
            {
                return send_message(agentUri, "hub.hello",
                    () => client.HelloFrom(helloAgentUri, snapshotId));
            }
        }

        bool IMessengerEngine.SendGoodbyeMsg(string agentUri, string goodbyeAgentUri)
        {
            using (var client = create_client_proxy(agentUri))
            {
                return send_message(agentUri, "hub.goodbye",
                    () => client.GoodbyeFrom(goodbyeAgentUri));
            }
        }

        bool IMessengerEngine.ClearAgentsList(string agentUri)
        {
            using (var client = create_client_proxy(agentUri))
            {
                return send_message(agentUri, "hub.clearAgents",
                    () => client.ClearAgentsList());
            }
        }

        void IMessengerEngine.FilterTopicMsgs(string agentUri, comm.TopicFilterMsg msg)
        {
            using (var client = create_client_proxy(agentUri))
            {
                send_message(agentUri, "hub.FilterTopic",
                    () => client.FilterTopicMsgs(msg));
            }
        }

        ExpandoObject IHaveDump.Dump(dynamic root)
        {
            if (root == null)
                root = new ExpandoObject();

            return root;
        }

        #endregion
    }
}