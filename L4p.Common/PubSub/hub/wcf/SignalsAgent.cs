using System;
using L4p.Common.PubSub.comm;
using L4p.Common.Wcf;

namespace L4p.Common.PubSub.hub.wcf
{
    interface ISignalsAgent : 
        comm.ISignalsAgent, IDisposable
    {}

    class SignalsAgent
        : WcfProxy<ISignalsAgent>, ISignalsAgent
    {
        public static ISignalsAgent New(string uri)
        {
            return new SignalsAgent(uri);
        }

        private SignalsAgent(string uri) 
            : base(uri) 
        { }

        void comm.ISignalsAgent.HelloFrom(string agentUri, int snapshotId)
        {
            Channel.HelloFrom(agentUri, snapshotId);
        }

        void comm.ISignalsAgent.GoodbyeFrom(string agentUri)
        {
            Channel.GoodbyeFrom(agentUri);
        }

        void comm.ISignalsAgent.Publish(PublishMsg msg)
        {
            Channel.Publish(msg);
        }

        void comm.ISignalsAgent.ClearAgentsList()
        {
            Channel.ClearAgentsList();
        }

        void comm.ISignalsAgent.FilterTopicMsgs(comm.TopicFilterMsg msg)
        {
            Channel.FilterTopicMsgs(msg);
        }

        void comm.ISignalsAgent.Heartbeat()
        {
            Channel.Heartbeat();
        }
    }
}