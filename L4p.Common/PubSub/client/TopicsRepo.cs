using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using L4p.Common.DumpToLogs;

namespace L4p.Common.PubSub.client
{
    class TopicCounters
    {
        public int Handlers;
        public int Filters;
        public int MsgPublished;
        public int MsgGotPublished;
        public int NoHandlersFound;
        public int MsgDispatched;
        public int FiledToBuildFilterTopicMsg;
    }

    class TopicDetails
    {
        public string TopicName { get; set; }
        public TopicCounters Counters { get; set; }

        public TopicDetails()
        {
            Counters = new TopicCounters();
        }
    }

    interface ITopicsRepo : IHaveDump
    {
        TopicDetails GetTopicDetails(Guid topicGuid, string topicName);
    }

    class TopicsRepo : ITopicsRepo
    {
        #region members

        private readonly Dictionary<Guid, TopicDetails> _topics;

        #endregion

        #region construction

        public static ITopicsRepo New()
        {
            return
                new TopicsRepo();
        }

        public static ITopicsRepo NewSync()
        {
            return
                SyncTopicsRepo.New(
                new TopicsRepo());
        }

        private TopicsRepo()
        {
            _topics = new Dictionary<Guid, TopicDetails>();
        }

        #endregion

        #region private
        #endregion

        #region interface

        TopicDetails ITopicsRepo.GetTopicDetails(Guid topicGuid, string topicName)
        {
            TopicDetails details;

            if (_topics.TryGetValue(topicGuid, out details))
                return details;

            details = new TopicDetails {TopicName = topicName};
            _topics.Add(topicGuid, details);

            return details;
        }

        ExpandoObject IHaveDump.Dump(dynamic root)
        {
            if (root == null)
                root = new ExpandoObject();

            root.Count = _topics.Count;
            root.Details = _topics.Values.ToArray();

            return root;
        }

        #endregion
    }

    class SyncTopicsRepo : ITopicsRepo
    {
        private readonly object _mutex = new object();
        private readonly ITopicsRepo _impl;

        public static ITopicsRepo New(ITopicsRepo impl) { return new SyncTopicsRepo(impl); }
        private SyncTopicsRepo(ITopicsRepo impl) { _impl = impl; }

        TopicDetails ITopicsRepo.GetTopicDetails(Guid topicGuid, string topicName) { lock (_mutex) return _impl.GetTopicDetails(topicGuid, topicName); }
        ExpandoObject IHaveDump.Dump(dynamic root) { lock (_mutex) return _impl.Dump(); }
    }

}