using System;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace L4p.Common.PubSub.comm
{
    [DataContract]
    public class HelloMsg
    {
        [DataMember] public string AgentUri { get; set; }
        [DataMember] public int SnapshotId { get; set; }
        [DataMember] public string ServiceName { get; set; }
        [DataMember] public int SequentialId { get; set; }
    }

    [ServiceContract]
    interface ISignalsHub : IDisposable
    {
        /// <summary>
        /// Say hello to the hub. 
        /// Can't be one way since hub may response with ClearAgentsList()
        /// </summary>
        [OperationContract]
        void Hello(HelloMsg msg);

        [OperationContract]
        [OneWay]
        void Goodbye(string agentUri);
    }
}