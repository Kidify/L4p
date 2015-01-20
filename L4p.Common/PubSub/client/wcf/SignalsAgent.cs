using System;
using System.ServiceModel;
using L4p.Common.PubSub.comm;

namespace L4p.Common.PubSub.client.wcf
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple,
        IncludeExceptionDetailInFaults = true
    )]
    class SignalsAgent : comm.ISignalsAgent
    {
        #region members

        private readonly ISignalsAgent _target;

        #endregion

        #region construction

        public static comm.ISignalsAgent New(ISignalsAgent target)
        {
            return
                new SignalsAgent(target);
        }

        private SignalsAgent(ISignalsAgent target)
        {
            _target = target;
        }

        #endregion

        #region interface

        void comm.ISignalsAgent.HelloFrom(string agentUri, int snapshotId)
        {
            _target.HelloFrom(agentUri, snapshotId);
        }

        void comm.ISignalsAgent.GoodbyeFrom(string agentUri)
        {
            _target.GoodbyeFrom(agentUri);
        }

        void comm.ISignalsAgent.Publish(PublishMsg msg)
        {
            _target.Publish(msg);
        }

        void comm.ISignalsAgent.ClearAgentsList()
        {
            _target.ClearAgentsList();
        }

        void comm.ISignalsAgent.FilterTopicMsgs(TopicFilterMsg msg)
        {
            _target.FilterTopicMsgs(msg);
        }

        void comm.ISignalsAgent.Heartbeat()
        {
            _target.Heartbeat();
        }

        #endregion
    }
}