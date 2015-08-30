using System;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.ServiceModel;
using L4p.Common.PubSub.client;

namespace L4p.Common.PubSub.comm
{
    [DataContract]
    class IoMsg
    {
        [DataMember] public Guid Guid { get; set; }
        public string AgentUri { get; set; }
        public Action Retry { get; set; }
        public int RetryCount { get; set; }
    }

    [DataContract]
    class PublishMsg : IoMsg
    {
        public Topic Topic { get; set; }
        [DataMember] public string TopicName { get; set; }
        [DataMember] public Guid TopicGuid { get; set; }
        [DataMember] public string Json { get; set; }
        [DataMember] public string FromAgent { get; set; }
    }

    [DataContract]
    class FilterInfo : IoMsg
    {
        [DataMember] public bool IsTransferable { get; set; }
        [DataMember] public string ModuleName { get; set; }
        [DataMember] public int MethodToken { get; set; }
        [DataMember] public string DefinedAt { get; set; }
        [DataMember] public string ContextAsJson { get; set; }
    }

    /// <summary>
    /// When filters are empty that means the whole topic is filtered
    /// </summary>
    [DataContract]
    class TopicFilterMsg : IoMsg
    {
        [DataMember] public string AgentToFilter { get; set; }
        [DataMember] public int SnapshotId { get; set; }
        [DataMember] public Guid TopicGuid { get; set; }
        [DataMember] public string TopicName { get; set; }
        [DataMember] public FilterInfo[] Filters { get; set; }
    }

    [DataContract]
    class HeartbeatMsg : IoMsg
    {}

    [ServiceContract]
    interface ISignalsAgent
    {
        [OperationContract]
        [OneWay]
        void HelloFrom(string agentUri, int snapshotId);

        [OperationContract]
        [OneWay]
        void GoodbyeFrom(string agentUri);

        [OperationContract(Action = "Signals/Publish", ReplyAction = "Signals/PublishReply")]
        [OneWay]
        void Publish(PublishMsg msg);

        /// <summary>
        /// ClearAgentsList() is triggered as a response to a first Hello() message.
        /// It can't be one-way to prevent a race condition of consequent agent hello messages
        /// </summary>
        [OperationContract]
        void ClearAgentsList();

        [OperationContract(Action = "Signals/FilterTopicMsgs", ReplyAction = "Signals/FilterTopicMsgsReply")]
        [OneWay]
        void FilterTopicMsgs(TopicFilterMsg msg);

        [OperationContract(Action = "Signals/Heartbeat", ReplyAction = "Signals/HeartbeatReply")]
        [OneWay]
        void Heartbeat();
    }

    [ServiceContract]
    interface IAgentAsyncWriter
    {
        [OperationContract(Action = "Signals/Publish", ReplyAction = "Signals/PublishReply", AsyncPattern = true)]
        [OneWay]
        IAsyncResult BeginPublish(PublishMsg msg, AsyncCallback callback, object state);
        void EndPublish(IAsyncResult ar);

        [OperationContract(Action = "Signals/FilterTopicMsgs", ReplyAction = "Signals/FilterTopicMsgsReply", AsyncPattern = true)]
        [OneWay]
        IAsyncResult BeginFilterTopicMsgs(TopicFilterMsg msg, AsyncCallback callback, object state);
        void EndFilterTopicMsgs(IAsyncResult ar);

        [OperationContract(Action = "Signals/Heartbeat", ReplyAction = "Signals/HeartbeatReply", AsyncPattern = true)]
        [OneWay]
        IAsyncResult BeginHeartbeat(AsyncCallback callback, object state);
        void EndHeartbeat(IAsyncResult ar);
    }
}