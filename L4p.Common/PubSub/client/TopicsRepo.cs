using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using L4p.Common.DumpToLogs;
using L4p.Common.Extensions;
using L4p.Common.Json;

namespace L4p.Common.PubSub.client
{
    class TopicCounters
    {
        public long MsgPublished;
        public long GotPublishedMsg;
        public long SkippedNoListenersFound;
        public long IoFailed;
        public long MsgNotSent;
        public long Handlers;
        public long Filters;
        public long NoHandlersFound;
        public long MsgDispatched;
        public long FailedToBuildFilterTopicMsg;
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

            var details =
                from topic in _topics.Values
                orderby topic.TopicName
                let counters = topic.Counters
                select "'{0}': published={1} got={2} skipped={3} ioFailed={4} notSent={5} handlers={6} filters={7} handlerNotFound={8} dispatched={9} FailedToBuildFilterTopicMsg={10}".Fmt(
                    topic.TopicName,
                    counters.MsgPublished,
                    counters.GotPublishedMsg,
                    counters.SkippedNoListenersFound,
                    counters.IoFailed,
                    counters.MsgNotSent,
                    counters.Handlers,
                    counters.Filters,
                    counters.NoHandlersFound,
                    counters.MsgDispatched,
                    counters.FailedToBuildFilterTopicMsg);

            root.Count = _topics.Count;
            root.Details = details.ToArray();

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