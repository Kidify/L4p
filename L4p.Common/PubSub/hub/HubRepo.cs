using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using L4p.Common.DumpToLogs;
using L4p.Common.Extensions;
using L4p.Common.Helpers;
using L4p.Common.Loggers;

namespace L4p.Common.PubSub.hub
{
    class AgentInfo
    {
        public string AgentUri { get; set; }
        public int SnapshotId { get; set; }
        public string ServiceName { get; set; }
        public DateTime LastUpdateAt { get; set; }
        public int ClearAgentsListIsSent;
        public int ClearAgentsListIsCompleted;
    }

    interface IHubRepo : IHaveDump
    {
        AgentInfo GetAgent(string agentUri);
        void SetConsequentSnapshotId(AgentInfo agent, int snapshotId, DateTime now);
        AgentInfo[] GetAllAgents();
        AgentInfo[] QueryDirtyAgents(AgentInfo refAgent, int snapshotId);
        AgentInfo[] QueryDeadAgents(DateTime now, TimeSpan ttl);
        void AgentsAreUpdated(AgentInfo refAgent, AgentInfo[] cleanAgents, int snapshotId);
        void RemoveAgent(string agentUri);
    }

    class HubRepo : IHubRepo
    {
        #region members

        private readonly Dictionary<string, AgentInfo> _agents;
        private readonly Dictionary<string, List<string>> _state;

        #endregion

        #region construction

        public static IHubRepo New()
        {
            return
                new HubRepo();
        }

        public static IHubRepo NewSync()
        {
            return
                SyncHubRepo.New(
                new HubRepo());
        }

        private HubRepo()
        {
            _agents = new Dictionary<string, AgentInfo>();
            _state = new Dictionary<string, List<string>>();
        }

        #endregion

        #region private

        private AgentInfo new_agent_info(string agentUri)
        {
            var agent = new AgentInfo
                {
                    AgentUri = agentUri,
                    SnapshotId = 0
                };

            _agents.Add(agentUri, agent);
            _state[agentUri] = null;

            return agent;
        }

        private AgentInfo get_agent(string agentUri)
        {
            AgentInfo agent;

            if (_agents.TryGetValue(agentUri, out agent))
                return agent;

            agent = new_agent_info(agentUri);

            return agent;
        }

        private List<string> get_agent_state(AgentInfo agent)
        {
            List<string> list;

            _state.TryGetValue(agent.AgentUri, out list);

            if (list != null)
                return list;

            list = new List<string>();
            _state[agent.AgentUri] = list;

            return list;
        }

        private void add_clean_agent_to_state(List<string> list, string agentUri)
        {
            if (list.Contains(agentUri))
                return;

            list.Add(agentUri);
        }

        #endregion

        #region interface

        AgentInfo IHubRepo.GetAgent(string agentUri)
        {
            var agent = get_agent(agentUri);
            return agent;
        }

        void IHubRepo.SetConsequentSnapshotId(AgentInfo agent, int snapshotId, DateTime now)
        {
            Validate.NotNull(agent);

            agent.LastUpdateAt = now;

            if (agent.SnapshotId >= snapshotId)
                return;

            agent.SnapshotId = snapshotId;
            _state[agent.AgentUri] = null;
        }

        AgentInfo[] IHubRepo.GetAllAgents()
        {
            return
                _agents.Values.ToArray();
        }

        AgentInfo[] IHubRepo.QueryDirtyAgents(AgentInfo refAgent, int snapshotId)
        {
            var allAgents = _agents.Values.ToArray();
            var cleanAgents = get_agent_state(refAgent);

            Validate.NotNull(cleanAgents);

            var query =
                from agent in allAgents
                where !cleanAgents.Contains(agent.AgentUri)
                select agent;

            var dirtyAgents = query.ToArray();
            return dirtyAgents;
        }

        AgentInfo[] IHubRepo.QueryDeadAgents(DateTime now, TimeSpan ttl)
        {
            var aliveFromHere = now - ttl;

            var query =
                from agent in _agents.Values
                where agent.LastUpdateAt < aliveFromHere
                select agent;

            return
                query.ToArray();
        }

        void IHubRepo.AgentsAreUpdated(AgentInfo refAgent, AgentInfo[] cleanAgents, int snapshotId)
        {
            if (refAgent.SnapshotId > snapshotId)
                return;

            var list = get_agent_state(refAgent);

            foreach (var agent in cleanAgents)
            {
                add_clean_agent_to_state(list, agent.AgentUri);
            }
        }

        void IHubRepo.RemoveAgent(string agentUri)
        {
            _agents.Remove(agentUri);
            _state.Remove(agentUri);

            foreach (var list in _state.Values)
            {
                list.Remove(agentUri);
            }
        }

        ExpandoObject IHaveDump.Dump(dynamic root)
        {
            if (root == null)
                root = new ExpandoObject();

            var agents = _agents.Values.ToArray();
            var details = new List<object>();

            foreach (var agent in agents)
            {
                var list = get_agent_state(agent);

                if (list == null)
                    continue;

                var entry = new
                    {
                        Agent = agent,
                        State = new
                            {
                                Count = list.Count,
                                Agents = list.ToArray()   
                            }
                    };

                details.Add(entry);
            }

            root.AgentsCount = agents.Length;
            root.Agents = new
                {
                    Count = details.Count,
                    Items = details.ToArray()
                };

            return root;
        }

        #endregion
    }

    class SyncHubRepo : IHubRepo
    {
        private readonly object _mutex = new object();
        private readonly IHubRepo _impl;

        public static IHubRepo New(IHubRepo impl) { return new SyncHubRepo(impl); }
        private SyncHubRepo(IHubRepo impl) { _impl = impl; }

        AgentInfo IHubRepo.GetAgent(string agentUri) { lock (_mutex) return _impl.GetAgent(agentUri); }
        void IHubRepo.SetConsequentSnapshotId(AgentInfo agent, int snapshotId, DateTime now) { lock (_mutex) _impl.SetConsequentSnapshotId(agent, snapshotId, now); }
        AgentInfo[] IHubRepo.GetAllAgents() { lock (_mutex) return _impl.GetAllAgents(); }
        AgentInfo[] IHubRepo.QueryDirtyAgents(AgentInfo refAgent, int snapshotId) { lock (_mutex) return _impl.QueryDirtyAgents(refAgent, snapshotId); }
        AgentInfo[] IHubRepo.QueryDeadAgents(DateTime now, TimeSpan ttl) { lock (_mutex) return _impl.QueryDeadAgents(now, ttl); }
        void IHubRepo.AgentsAreUpdated(AgentInfo refAgent, AgentInfo[] cleanAgents, int snapshotId) { lock (_mutex) _impl.AgentsAreUpdated(refAgent, cleanAgents, snapshotId); }
        void IHubRepo.RemoveAgent(string agentUri) { lock(_mutex) _impl.RemoveAgent(agentUri); }
        ExpandoObject IHaveDump.Dump(dynamic root) { lock (_mutex) return _impl.Dump(); }
    }
}