using System;
using System.Dynamic;
using System.Threading;
using L4p.Common.DumpToLogs;
using L4p.Common.Extensions;
using L4p.Common.IoCs;
using L4p.Common.Json;
using L4p.Common.Loggers;
using L4p.Common.PubSub.client;
using L4p.Common.Schedulers;
using L4p.Common.Wcf;

namespace L4p.Common.PubSub.hub
{
    public interface ISignalsHub : IHaveDump
    {
        void Hello(comm.HelloMsg msg);
        void Goodbye(string agentUri);

        void Start();
        void Idle();
        void Stop();
    }

    public class SignalsHub : ISignalsHub
    {
        #region counters

        class Counters
        {
            public int HelloMsgsGot;
            public int HelloMsgsSent;
            public int GoodbyeMsgsGot;
            public int GoodbyeMsgsSent;
            public int AgentIsGoneWarns;
            public int AgentsGone;
            public int NewAgents;
            public int Dumps;
        }

        #endregion

        #region members

        private readonly string _serviceName;
        private readonly Counters _counters;
        private readonly ILogFile _log;
        private readonly ISignalsConfigRa _configRa;
        private readonly IHubRepo _repo;
        private readonly IMessangerEngine _messanger;
        private readonly IAgentsEngine _agents;
        private readonly IWcfHost _host;
        private readonly IIdler _idler;
        private readonly IEventScheduler _scheduler;

        #endregion

        #region construction

        public static ISignalsHub New(SignalsConfig config = null)
        {
            config = config ?? new SignalsConfig();

            return
                new SignalsHub(config);
        }

        private static IIoC create_dependencies(SignalsConfig config)
        {
            var ioc = IoC.New();

            var log = LogFile.New(SignalsComponent.LogName);
            var configRa = SignalsConfigRa.New(config);
            var repo = HubRepo.NewSync();
            var idler = Idler.New(log);
            var scheduler = EventScheduler.New(log);

            var throttledLog = ThrottledLog.NewSync(config.ThrottledLogTtl, log);
            var messanger = MessangerEngine.New(throttledLog);

            ioc.Setup()
                .Register(log)
                .Register(configRa)
                .Register(repo)
                .Register(messanger)
                .Register(idler)
                .Register(scheduler);

            var agents = AgentsEngine.New(ioc);

            ioc.Setup()
                .Register(agents);

            return ioc;
        }

        private SignalsHub(SignalsConfig config)
        {
            _counters = new Counters();
            var ioc = create_dependencies(config);

            _log = ioc.Resolve<ILogFile>();
            _repo = ioc.Resolve<IHubRepo>();
            _configRa = ioc.Resolve<ISignalsConfigRa>();
            _messanger = ioc.Resolve<IMessangerEngine>();
            _agents = ioc.Resolve<IAgentsEngine>();
            _idler = ioc.Resolve<IIdler>();
            _scheduler = ioc.Resolve<IEventScheduler>();

            var target = wcf.SignalsHub.New(this);
            _host = WcfHost<comm.ISignalsHub>.NewAsync(_log, target);
            _serviceName = GetType().AsServiceName();
        }

        #endregion

        #region private

        private void agent_is_gone(string agentUri)
        {
            _repo.RemoveAgent(agentUri);

            _log.Info("Agent at '{0}' said goodbye; sending goodbye messages to all", agentUri);

            var agents = _repo.GetAllAgents();

            if (agents.IsEmpty())
                return;

            _agents.send_goodbye_msgs(agents, agentUri);
            Interlocked.Add(ref _counters.GoodbyeMsgsSent, agents.Length);
        }

        private void remove_dead_agents(AgentInfo[] agents, TimeSpan deadAgentTtl)
        {
            foreach (var agent in agents)
            {
                _log.Info("Agent at '{0}' is dead (ttl='{1}'; sending goodbye message)", agent.AgentUri, deadAgentTtl);
                agent_is_gone(agent.AgentUri);
            }
        }

        private void clear_remote_agents_on_first_hello(AgentInfo agent)
        {
            if (Interlocked.CompareExchange(ref agent.ClearAgentsListIsSent, 1, 0) > 0)
                return;

            Interlocked.Increment(ref _counters.NewAgents);
            _messanger.ClearAgentsList(agent.AgentUri);

            agent.ClearAgentsListIsCompleted = 1;
        }

        private static void dump_state_to_log(ISignalsHub self, ILogFile log)
        {
            var dump = self.Dump().ToJson();
            log.Trace("SugnalsHub dump: {0}", dump);
        }

        #endregion

        #region interface

        void ISignalsHub.Hello(comm.HelloMsg msg)
        {
//            _log.Trace("Client's hello msg no {0} snapshotId {1} from '{2}'", msg.SequentialId, msg.SnapshotId, msg.AgentUri);

            var now = DateTime.UtcNow;

            var snapshotId = msg.SnapshotId;

            Interlocked.Increment(ref _counters.HelloMsgsGot);

            var agent = _repo.GetAgent(msg.AgentUri);
            agent.ServiceName = msg.ServiceName;

            clear_remote_agents_on_first_hello(agent);

            _repo.SetConsequentSnapshotId(agent, snapshotId, now);

            var dirtyAgents = _repo.QueryDirtyAgents(agent, snapshotId);

            if (dirtyAgents.IsEmpty())
                return;

            _log.Info("Hello no={0} snapshotId={1} from '{2}'; agent has {3} dirty agents", 
                msg.SequentialId, snapshotId, msg.AgentUri, dirtyAgents.Length);

            var cleanAgents = _agents.send_hello_msgs(dirtyAgents, agent, snapshotId);
            _repo.AgentsAreUpdated(agent, cleanAgents, snapshotId);

            Interlocked.Add(ref _counters.HelloMsgsSent, cleanAgents.Length);
        }

        void ISignalsHub.Goodbye(string agentUri)
        {
            agent_is_gone(agentUri);
            Interlocked.Increment(ref _counters.GoodbyeMsgsGot);
        }

        void ISignalsHub.Start()
        {
            var config = _configRa.Values;

            _log.Info("Starting signals hub at '{0}'", _serviceName);

            var hubUri = _configRa.MakeHubUri();
            _host.StartAt(hubUri);
            _scheduler.Start();

            ISignalsHub self = this;
            _idler.Idle(config.Hub.IdleSpan, self.Idle);

            _scheduler.Repeat(config.Hub.DumpToLogPeriod,
                              () => dump_state_to_log(this, _log));

            _log.Info("SignalsHub is started at '{0}'; {1}", _serviceName, config.ToJson());
        }

        void ISignalsHub.Idle()
        {
            var now = DateTime.UtcNow;
            var config = _configRa.Values;

            {
                var agents = _repo.QueryDeadAgents(now, config.Hub.DeadAgentTtl);
                remove_dead_agents(agents, config.Hub.DeadAgentTtl);
                Interlocked.Add(ref _counters.AgentsGone, agents.Length);
            }

            {
                var agents = _repo.QueryDeadAgents(now, config.Hub.InactiveAgentTtl);
                _agents.warn_inactive_agents(agents);
                Interlocked.Add(ref _counters.AgentIsGoneWarns, agents.Length);
            }
        }

        void ISignalsHub.Stop()
        {
            _log.Info("Stopping signals hub at '{0}'", _serviceName);

            _idler.Stop();
            _host.Close();

            _log.Info("SignalsHub is stopped at '{0}'", _serviceName);
        }

        ExpandoObject IHaveDump.Dump(dynamic root)
        {
            if (root == null)
                root = new ExpandoObject();

            root.ServiceName = _serviceName;
            root.Counters = _counters;
            root.Config = _configRa.Values;
            root.Repo = _repo.Dump();
//            root.Messanger = _messanger.Dump();

            Interlocked.Increment(ref _counters.Dumps);

            return root;
        }

        #endregion
    }
}