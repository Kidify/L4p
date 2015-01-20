using System.Collections.Generic;
using L4p.Common.Json;
using L4p.Common.IoCs;
using L4p.Common.Loggers;

namespace L4p.Common.PubSub.hub
{
    interface IAgentsEngine
    {
        AgentInfo[] send_hello_msgs(AgentInfo[] agents, AgentInfo helloAgent, int snapshotId);
        void send_goodbye_msgs(AgentInfo[] agents, string goodbyeAgent);
        void warn_inactive_agents(AgentInfo[] agents);
    }

    class AgentsEngine : IAgentsEngine
    {
        #region members

        private readonly ILogFile _log;
        private readonly IHubRepo _repo;
        private readonly IMessangerEngine _messanger;

        #endregion

        #region construction

        public static IAgentsEngine New(IIoC ioc)
        {
            return
                new AgentsEngine(ioc);
        }

        private AgentsEngine(IIoC ioc)
        {
            _log = ioc.Resolve<ILogFile>();
            _repo = ioc.Resolve<IHubRepo>();
            _messanger = ioc.Resolve<IMessangerEngine>();
        }

        #endregion

        #region private
        #endregion

        #region interface

        AgentInfo[] IAgentsEngine.send_hello_msgs(AgentInfo[] agents, AgentInfo helloAgent, int snapshotId)
        {
            var cleanAgents = new List<AgentInfo>();
            cleanAgents.Add(helloAgent);

            foreach (var agent in agents)
            {
                if (agent.ClearAgentsListIsCompleted == 0)
                    continue;

                bool sent = _messanger.SendHelloMsg(agent.AgentUri, helloAgent.AgentUri, snapshotId);

                if (sent)
                    cleanAgents.Add(agent);
            }

            return
                cleanAgents.ToArray();
        }

        void IAgentsEngine.send_goodbye_msgs(AgentInfo[] agents, string goodbyeAgent)
        {
            foreach (var agent in agents)
            {
                if (agent.AgentUri == goodbyeAgent)
                    continue;

                _messanger.SendGoodbyeMsg(agent.AgentUri, goodbyeAgent);
            }
        }

        void IAgentsEngine.warn_inactive_agents(AgentInfo[] agents)
        {
            foreach (var agent in agents)
            {
                _log.Trace("Agent is inactive; {0}", agent.AsSingleLineJson());
            }
        }

        #endregion
    }
}