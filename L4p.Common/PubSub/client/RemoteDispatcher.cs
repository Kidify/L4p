using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using L4p.Common.DumpToLogs;
using L4p.Common.Extensions;
using L4p.Common.Helpers;
using L4p.Common.IoCs;
using L4p.Common.Json;
using L4p.Common.Loggers;
using L4p.Common.PubSub.utils;

namespace L4p.Common.PubSub.client
{
    interface IRemoteDispatcher : IHaveDump
    {
        bool DispatchRemoteMsg(Topic topic, object msg);
        bool FilterTopicMsgs(string target, comm.TopicFilterMsg msg);
    }

    class RemoteDispatcher : IRemoteDispatcher
    {
        #region counters

        class Counters
        {
            public int MsgsDispatched;
            public int MsgsFiltered;
            public int FilterTopicMsgsSent;
        }

        #endregion

        #region members

        private readonly string _myself;
        private readonly Counters _counters;
        private readonly ILogFile _log;
        private readonly IJsonEngine _json;
        private readonly IRemoteRepo _rrepo;
        private readonly IMessengerEngine _messenger;

        #endregion

        #region construction

        public static IRemoteDispatcher New(string myself, IIoC ioc)
        {
            return
                new RemoteDispatcher(myself, ioc);
        }

        private RemoteDispatcher(string myself, IIoC ioc)
        {
            _myself = myself;
            _counters = new Counters();
            _log = ioc.Resolve<ILogFile>();
            _json = ioc.Resolve<IJsonEngine>();
            _rrepo = ioc.Resolve<IRemoteRepo>();
            _messenger = ioc.Resolve<IMessengerEngine>();
        }

        #endregion

        #region private

        private bool filter_is_matched(PFilterFunc[] filters, object msg)
        {
            if (filters.IsEmpty())
                return false;

            foreach (var filter in filters)
            {
                var filterIsMatched = filter(msg);

                if (filterIsMatched)
                    return true;
            }

            return false;
        }

        private FilteredTopic get_topic_filter(Guid topicGuid, RemoteAgent agent)
        {
            var all = _rrepo.GetFilteredTopics(agent);

            if (all == null)
                return null;

            var query =
                from filter in all
                where filter.TopicGuid == topicGuid
                select filter;

            return
                query.FirstOrDefault();
        }

        private bool msg_shoould_be_sent(Topic topic, RemoteAgent agent, object msg)
        {
            var topicFilter = get_topic_filter(topic.Guid, agent);

            if (topicFilter == null)
                return true;

            if (topicFilter.Filters.IsEmpty())
                return false;               // topic has no listeners on target

            if (topicFilter.FilterThrows)
                return true;                // there is a local filter but it throws (remote will recalculate filter)

            try
            {
                var filterIsMatched = filter_is_matched(topicFilter.Filters, msg);

                if (filterIsMatched)
                    return true;
            }
            catch (Exception ex)
            {
                topicFilter.FilterThrows = true;
                _log.Warn(ex, "Failed to invoke remote filter for topic {0}", topic.ToJson());
            }

            // no matched filters

            return false;
        }

        private void dispatch_to_remote(string agent, Topic topic, string json)
        {
            var msg = new comm.PublishMsg {
                Topic = topic,
                TopicName = topic.Name,
                TopicGuid = topic.Guid,
                Json = json,
                FromAgent = _myself
            };

            _messenger.SendPublishMsg(agent, msg);
        }

        #endregion

        #region interface

        bool IRemoteDispatcher.DispatchRemoteMsg(Topic topic, object msg)
        {
            var agents = _rrepo.GetAgents();

            if (agents.IsEmpty())
                return false;

            var json = _json.MsgToJson(msg);
            var hasListeners = false;

            foreach (var agent in agents)
            {
                bool msgShouldBeSent = msg_shoould_be_sent(topic, agent, msg);

                if (msgShouldBeSent == false)
                {
                    Interlocked.Increment(ref _counters.MsgsFiltered);
                    continue;
                }

                hasListeners = true;

                dispatch_to_remote(agent.AgentUri, topic, json);
                Interlocked.Increment(ref _counters.MsgsDispatched);
            }

            return hasListeners;
        }

        bool IRemoteDispatcher.FilterTopicMsgs(string target, comm.TopicFilterMsg msg)
        {
            _messenger.FilterTopicMsgs(target, msg);
            Interlocked.Increment(ref _counters.FilterTopicMsgsSent);
            return true;
        }

        ExpandoObject IHaveDump.Dump(dynamic root)
        {
            if (root == null)
                root = new ExpandoObject();

            root.MySelf = _myself;
            root.Counters = _counters;

            return root;
        }

        #endregion
    }
}