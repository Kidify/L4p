using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using L4p.Common.DumpToLogs;
using L4p.Common.Extensions;
using L4p.Common.PubSub.utils;

namespace L4p.Common.PubSub.client
{
    class RemoteAgent
    {
        public string AgentUri { get; set; }
        public int SnapshotId { get; set; }
        public List<FilteredTopic> FilteredTopics { get; set; }
    }

    class FilteredTopic
    {
        public Guid TopicGuid { get; set; }
        public string TopicName { get; set; }
        public PFilterFunc[] Filters { get; set; }
        public comm.FilterInfo[] FilterInfos { get; set; }
        public bool FilterThrows { get; set; }
    }

    interface IRemoteRepo : IHaveDump
    {
        void SetSnapshotId(string agentUri, int snapshotId);
        void RemoveAgent(string agentUri);
        RemoteAgent[] GetAgents();
        FilteredTopic[] GetFilteredTopics(RemoteAgent agent);
        void FilterTopicMsgs(comm.TopicFilterMsg msg, PFilterFunc[] filters);
        void Clear();
    }

    class RemoteRepo : IRemoteRepo
    {
        #region members

        private readonly Dictionary<string, RemoteAgent> _remotes;

        #endregion

        #region construction

        public static IRemoteRepo New()
        {
            return
                SyncRemoteRepo.New(
                new RemoteRepo());
        }

        private RemoteRepo()
        {
            _remotes = new Dictionary<string, RemoteAgent>();
        }

        #endregion

        #region private

        private RemoteAgent get_agent(string agentUri)
        {
            RemoteAgent agent;

            if (_remotes.TryGetValue(agentUri, out agent))
                return agent;

            agent = new RemoteAgent
                {
                    AgentUri = agentUri,
                    SnapshotId = 0,
                };

            _remotes.Add(agentUri, agent);

            return agent;
        }

        #endregion

        #region interface

        void IRemoteRepo.SetSnapshotId(string agentUri, int snapshotId)
        {
            var agent = get_agent(agentUri);

            if (agent.SnapshotId >= snapshotId)
                return;

            agent.SnapshotId = snapshotId;
            agent.FilteredTopics = null;
        }

        void IRemoteRepo.RemoveAgent(string agentUri)
        {
            _remotes.Remove(agentUri);
        }

        RemoteAgent[] IRemoteRepo.GetAgents()
        {
            return
                _remotes.Values.ToArray();
        }

        FilteredTopic[] IRemoteRepo.GetFilteredTopics(RemoteAgent agent)
        {
            if (agent.FilteredTopics == null)
                return null;

            return
                agent.FilteredTopics.ToArray();
        }

        void IRemoteRepo.FilterTopicMsgs(comm.TopicFilterMsg msg, PFilterFunc[] filters)
        {
            var agent = get_agent(msg.AgentToFilter);

            if (agent.SnapshotId > msg.SnapshotId)
                return;

            if (agent.FilteredTopics == null)
                agent.FilteredTopics = new List<FilteredTopic>();

            var topic = new FilteredTopic
                {
                    TopicGuid = msg.TopicGuid,
                    TopicName = msg.TopicName,
                    Filters = filters,
                    FilterInfos = msg.Filters
                };

            agent.FilteredTopics.Add(topic);
        }

        void IRemoteRepo.Clear()
        {
            _remotes.Clear();
        }

        ExpandoObject IHaveDump.Dump(dynamic root)
        {
            if (root == null)
                root = new ExpandoObject();

            var agents = new List<object>();

            foreach (var remote in _remotes.Values)
            {
                var topics = new List<object>();

                if (remote.FilteredTopics != null)
                foreach (var topic in remote.FilteredTopics)
                {
                    var filtersCount = 
                        topic.FilterInfos.IsEmpty() ? 0 : topic.FilterInfos.Length;

                    var entry = new
                        {
                            topic.TopicName,
                            topic.TopicGuid,
                            FiltersCount = filtersCount,
                            Filters = topic.FilterInfos
                        };

                    topics.Add(entry);
                }

                var agent = new
                    {
                        remote.AgentUri,
                        remote.SnapshotId,
                        FilteredTopics = topics
                    };

                agents.Add(agent);
            }

            root.Remotes = new
                {
                    Count = agents.Count,
                    Agents = agents
                };

            return root;
        }

        #endregion
    }

    class SyncRemoteRepo : IRemoteRepo
    {
        private readonly object _mutex = new object();
        private readonly IRemoteRepo _impl;

        public static IRemoteRepo New(IRemoteRepo impl) { return new SyncRemoteRepo(impl); }
        private SyncRemoteRepo(IRemoteRepo impl) { _impl = impl; }

        void IRemoteRepo.SetSnapshotId(string agentUri, int snapshotId) { lock (_mutex) _impl.SetSnapshotId(agentUri, snapshotId); }
        void IRemoteRepo.RemoveAgent(string agentUri) { lock (_mutex) _impl.RemoveAgent(agentUri); }
        RemoteAgent[] IRemoteRepo.GetAgents() { lock (_mutex) return _impl.GetAgents(); }
        FilteredTopic[] IRemoteRepo.GetFilteredTopics(RemoteAgent agent) { lock (_mutex) return _impl.GetFilteredTopics(agent); }
        void IRemoteRepo.FilterTopicMsgs(comm.TopicFilterMsg msg, PFilterFunc[] filters) { lock (_mutex) _impl.FilterTopicMsgs(msg, filters); }
        void IRemoteRepo.Clear() { lock (_mutex) _impl.Clear(); }
        ExpandoObject IHaveDump.Dump(dynamic root) { lock (_mutex) return _impl.Dump(); }
   }
}