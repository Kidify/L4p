using System;
using System.Diagnostics;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Threading;
using L4p.Common.DumpToLogs;
using L4p.Common.Extensions;
using L4p.Common.Helpers;
using L4p.Common.IoCs;
using L4p.Common.Json;
using L4p.Common.Loggers;
using L4p.Common.PubSub.client.Io;
using L4p.Common.PubSub.comm;
using L4p.Common.PubSub.contexts;
using L4p.Common.PubSub.utils;
using L4p.Common.Schedulers;

namespace L4p.Common.PubSub.client
{
    interface ISignalsManagerEx : ISignalsManager, IHaveDump
    {
        void GotHelloMsg(SignalsAgent asFriend, string agent, int snapshotId);
        void GotGoodbyeMsg(SignalsAgent asFriend, string agent);
        void GotPublishMsg(SignalsAgent asFriend, comm.PublishMsg pmsg);
        void ClearAgentsList(SignalsAgent asFriend);
        void FilterTopicMsgs(SignalsAgent asFriend, comm.TopicFilterMsg msg);

        void CancelSubscription(SignalSlot asFriend, HandlerInfo handler);
        void GenerateHelloMsg(HelloPulseBeat asFriend, int sequentialId);
        void Idle(SignalsManagerEx asFriend);
    }

    public class SignalsManagerEx : ISignalsManagerEx
    {
        #region counters

        class Counters
        {
            public int GotHelloMsg;
            public int GotGoodbyeMsg;
            public int GotPublishMsg;
            public int TopicIsNotFound;
            public int FailedToParseMsgJson;
            public int HelloMsgSent;
            public int GoodbyeMsgSent;
            public int PublishedMsgs;
            public int Subscribtions;
            public int SubscribtionsWithFilters;
            public int WholeTopicIsFiltered;
            public int FiltersAreSet;
            public int NoFiltersAreSet;
            public int FilterTopicMsgIsSent;
        }

        #endregion

        #region members

        private readonly string _serviceName;
        private readonly Counters _counters;
        private readonly ILogFile _log;
        private readonly ISignalsConfigRa _configRa;
        private readonly ILocalRepo _lrepo;
        private readonly IRemoteRepo _rrepo;
        private readonly ITopicsRepo _topics;
        private readonly ILocalDispatcher _locals;
        private readonly IRemoteDispatcher _remotes;
        private readonly IHeloPulseBeat _pulse;
        private readonly ISignalsAgent _agent;
        private readonly IMessangerEngine _messanger;
        private readonly IJsonEngine _json;
        private readonly ILocalFactory _factory;
        private readonly IEventScheduler _scheduler;
        private readonly ISessionContext _context;
        private readonly IFiltersEngine _filters;
        private readonly IAgentConnector _connector;

        #endregion

        #region construction

        public static ISignalsManager New(SignalsConfig config = null)
        {
            config = config ?? new SignalsConfig();

            return
                new SignalsManagerEx(config);
        }

        private IIoC create_dependencies(SignalsConfig config)
        {
            var ioc = IoC.New();

            var log = LogFile.New(SignalsComponent.LogName);
            var configRa = SignalsConfigRa.New(config);

            ioc.RegisterInstance<ILogFile>(log);
            ioc.RegisterInstance<ISignalsConfigRa>(configRa);
            ioc.RegisterInstance<ISignalsManagerEx>(this);

            var myAgent = SignalsAgent.New(ioc);

            ioc.SingleInstance(() => FiltersEngine.New(log));
            ioc.SingleInstance(LocalRepo.New);
            ioc.SingleInstance(RemoteRepo.New);
            ioc.SingleInstance(TopicsRepo.NewSync);
            ioc.SingleInstance(JsonEngine.New);
            ioc.SingleInstance(AgentConnector.New);
            ioc.SingleInstance(MessangerEngine.New);
            ioc.SingleInstance(LocalDispatcher.New);
            ioc.SingleInstance(() => RemoteDispatcher.New(myAgent.AgentUri, ioc));
            ioc.SingleInstance(() => HelloPulseBeat.New(ioc));
            ioc.SingleInstance(() => myAgent);
            ioc.SingleInstance(MessangerEngine.New);
            ioc.SingleInstance(LocalFactory.New);
            ioc.SingleInstance(() => EventScheduler.New(log));
            ioc.SingleInstance(SessionContext.New);

            return ioc;
        }

        private SignalsManagerEx(SignalsConfig config)
        {
            _serviceName = GetType().AsServiceName();

            var ioc = create_dependencies(config);

            _counters = new Counters();
            _log = ioc.Resolve<ILogFile>();
            _configRa = ioc.Resolve<ISignalsConfigRa>();
            _lrepo = ioc.Resolve<ILocalRepo>();
            _rrepo = ioc.Resolve<IRemoteRepo>();
            _topics = ioc.Resolve<ITopicsRepo>();
            _locals = ioc.Resolve<ILocalDispatcher>();
            _remotes = ioc.Resolve<IRemoteDispatcher>();
            _pulse = ioc.Resolve<IHeloPulseBeat>();
            _agent = ioc.Resolve<ISignalsAgent>();
            _messanger = ioc.Resolve<IMessangerEngine>();
            _json = ioc.Resolve<IJsonEngine>();
            _factory = ioc.Resolve<ILocalFactory>();
            _scheduler = ioc.Resolve<IEventScheduler>();
            _context = ioc.Resolve<ISessionContext>();
            _filters = ioc.Resolve<IFiltersEngine>();
            _connector = ioc.Resolve<IAgentConnector>();
        }

        #endregion

        #region private

        private bool it_is_myself(string agentUri)
        {
            return
                agentUri == _agent.AgentUri;
        }

        private void generate_hello_msg(int snapshotId, int sequentialId)
        {
            Validate.NotZero(snapshotId);

            var msg = new comm.HelloMsg
                {
                    AgentUri = _agent.AgentUri,
                    SnapshotId = snapshotId,
                    ServiceName = _serviceName,
                    SequentialId = sequentialId
                };

            _messanger.SendHelloMsg(msg);
            Interlocked.Increment(ref _counters.HelloMsgSent);
        }

        private void generate_goodbye_msg()
        {
            var agentUri = _agent.AgentUri;
            _messanger.SendGoodbyeMsg(agentUri);

            Interlocked.Increment(ref _counters.GoodbyeMsgSent);
        }

        private void say_deferred_hello()
        {
            var config = _configRa.Values;
            _pulse.SayHelloIn(config.Client.HelloMsgOnChangeSpan);
        }

        private static void register_dump_to_log(ISignalsManagerEx self, ILogFile log)
        {
            DumpManager.Register<SignalsComponent>(self.Dump);
        }

        private static void dump_state_to_log(ISignalsManagerEx self, ILogFile log)
        {
            var dump = self.Dump().ToJson();
            log.Info("SignalsManagerEx dump: {0}", dump);
        }

        private void filter_topic_msgs(int snapshotId, comm.PublishMsg pmsg, HandlerInfo[] handlers, TopicDetails topicDetails)
        {
            comm.TopicFilterMsg filterMsg = null;

            try
            {
                filterMsg = _filters.BuildFilterTopicMsg(_agent.AgentUri, snapshotId, pmsg, handlers, topicDetails);
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Failed to build filter topic message for {0}", pmsg.ToJson());
            }
                
            if (filterMsg == null)
            {
                Interlocked.Increment(ref topicDetails.Counters.FiledToBuildFilterTopicMsg);
                return;
            }

            _remotes.FilterTopicMsgs(pmsg.FromAgent, filterMsg);

            _log.Info("Topic '{0}': []--> filter to '{1}' {2}", pmsg.TopicName, _agent.AgentUri, filterMsg.AsDscrStr());
            Interlocked.Increment(ref _counters.FilterTopicMsgIsSent);
        }

        #endregion

        #region interface

        [MethodImpl(MethodImplOptions.NoInlining)]
        ISignalSlot ISignalsManager.SubscribeTo<T>(Action<T> callback, Func<T, bool> filter, StackFrame filterAt)
        {
            if (filterAt == null)
                filterAt = new StackFrame(1);

            var type = typeof(T);

            var topic = _factory.make_topic(type);
            var handler = _factory.make_handler(topic, callback, filter, filterAt);

            _lrepo.AddHandler(handler);
            Interlocked.Increment(ref topic.Details.Counters.Handlers);

            var slot = SignalSlot.New(this, handler);
            say_deferred_hello();

            Interlocked.Increment(ref _counters.Subscribtions);

            if (filter != null)
            {
                Interlocked.Increment(ref topic.Details.Counters.Filters);
                Interlocked.Increment(ref _counters.SubscribtionsWithFilters);
            }

            _log.Info("Topic '{0}': ({1}) is subscribed to", topic.Name, topic.Guid);

            return slot;
        }

        void ISignalsManager.Publish<T>(T msg)
        {
            Validate.NotNull(msg);

            var topic = _factory.make_topic(typeof(T));

            _log.Trace("Topic '{0}': []--> from '{1}'", topic.Name, _agent.AgentUri);
            Interlocked.Increment(ref topic.Details.Counters.MsgPublished);

            _locals.DispatchLocalMsg(topic, msg);
            _remotes.DispatchRemoteMsg(topic, msg);

            Interlocked.Increment(ref _counters.PublishedMsgs);
        }

        ISessionContext ISignalsManager.Context 
        {
            get { return _context; }
        }

        void ISignalsManager.StartAgent()
        {
            var config = _configRa.Values;

            _log.Trace("Starting signals manager at '{0}' ...", _serviceName);

            _agent.Start(this);
            _pulse.Start(this);
            _scheduler.Start();

            ISignalsManagerEx self = this;

            _scheduler.Repeat(config.Client.DumpToLogPeriod,
                () => dump_state_to_log(this, _log));

            _scheduler.Repeat(config.Client.IdleSpan,
                () => self.Idle(this));

            _scheduler.FireOnce(1.Seconds(), 
                () => register_dump_to_log(this, _log));

            _log.Info("Signals manager '{0}' is started at '{1}'; {2}", 
                SignalsComponent.Version, _serviceName, config.ToJson());
        }

        void ISignalsManager.StopAgent()
        {
            _log.Trace("Stopping signals manager at '{0}' ...", _serviceName);

            _scheduler.Stop();
            _pulse.Stop();
            _agent.Stop();

            generate_goodbye_msg();

            _log.Info("Signals manager is stopped at '{0}'", _serviceName);
        }

        ExpandoObject IHaveDump.Dump(dynamic root)
        {
            if (root == null)
                root = new ExpandoObject();

            var config = _configRa.Values;

            root.ServiceName = _serviceName;
            root.Counters = _counters;
            root.Config = config;
            root.LRepo = _lrepo.Dump();
            root.RRepo = _rrepo.Dump();
            root.LocalDispatcher = _locals.Dump();
            root.RemoteDispatcher = _remotes.Dump();
            root.Messanger = _messanger.Dump();
            root.PulseBeat = _pulse.Dump();
            root.LocalFactory = _factory.Dump();
            root.FiltersEngine = _filters.Dump();
            root.Topics = _topics.Dump();
            root.Connector = _connector.Dump();

            return root;
        }

        void ISignalsManagerEx.GotHelloMsg(SignalsAgent asFriend, string agent, int snapshotId)
        {
            if (it_is_myself(agent))
                return;

            _log.Info("Got hello from '{0}' snapshotId={1}", agent, snapshotId);
            Interlocked.Increment(ref _counters.GotHelloMsg);

            _rrepo.SetSnapshotId(agent, snapshotId);
            _messanger.AgentIsHere(agent);
        }

        void ISignalsManagerEx.GotGoodbyeMsg(SignalsAgent asFriend, string agent)
        {
            if (it_is_myself(agent))
                return;

            _log.Info("Got goodbye from '{0}'", agent);
            Interlocked.Increment(ref _counters.GotGoodbyeMsg);

            _rrepo.RemoveAgent(agent);
            _messanger.AgentIsGone(agent);
        }

        void ISignalsManagerEx.GotPublishMsg(SignalsAgent asFriend, comm.PublishMsg pmsg)
        {
            _log.Trace("Topic '{0}': -->[] from '{1}'", pmsg.TopicName, pmsg.FromAgent);
            Interlocked.Increment(ref _counters.GotPublishMsg);

            var topicDetails = _topics.GetTopicDetails(pmsg.TopicGuid, pmsg.TopicName);
            Interlocked.Increment(ref topicDetails.Counters.MsgGotPublished);

            int snapshotId;
            var handlers = _lrepo.GetHandlers(pmsg.TopicGuid, out snapshotId);

            if (handlers.IsEmpty())
            {
                Interlocked.Increment(ref _counters.TopicIsNotFound);
                Interlocked.Increment(ref topicDetails.Counters.NoHandlersFound);
                filter_topic_msgs(snapshotId, pmsg, null, topicDetails);
                return;
            }

            var topic = handlers[0].Topic;
            var msg = _json.MsgFromJson(topic, pmsg.Json);

            if (msg == null)
            {
                Interlocked.Increment(ref _counters.FailedToParseMsgJson);
                return;
            }

            bool hasListeners = _locals.DispatchRemoteMsg(msg, handlers);
            Interlocked.Increment(ref topicDetails.Counters.MsgDispatched);

            if (hasListeners)
                return;

            filter_topic_msgs(snapshotId, pmsg, handlers, topicDetails);
        }

        void ISignalsManagerEx.ClearAgentsList(SignalsAgent asFriend)
        {
            _rrepo.Clear();
            _log.Info("Got clear remote agents request (done)");
        }

        void ISignalsManagerEx.FilterTopicMsgs(SignalsAgent asFriend, comm.TopicFilterMsg msg)
        {
            _log.Trace("Topic '{0}': -->[] filter from '{1}'; {2}", msg.TopicName, msg.AgentToFilter, msg.AsDscrStr());

            if (msg.Filters.IsEmpty())
            {
                _rrepo.FilterTopicMsgs(msg, null);
                Interlocked.Increment(ref _counters.WholeTopicIsFiltered);
                return;
            }

            var filters = _filters.BuildFilters(msg.Filters);

            if (filters.IsEmpty())
            {
                Interlocked.Increment(ref _counters.NoFiltersAreSet);
                return;
            }

            _rrepo.FilterTopicMsgs(msg, filters);
            Interlocked.Increment(ref _counters.FiltersAreSet);
        }

        void ISignalsManagerEx.CancelSubscription(SignalSlot asFriend, HandlerInfo handler)
        {
            bool wasThere = _lrepo.RemoveHandler(handler);

            if (wasThere == false)
                return;

            _log.Trace("Topic '{0}': subscription is canceled", handler.Topic.Name);

            Interlocked.Decrement(ref _counters.Subscribtions);
            say_deferred_hello();
        }

        void ISignalsManagerEx.GenerateHelloMsg(HelloPulseBeat asFriend, int sequentialId)
        {
            Validate.NotNull(asFriend);

            int snapshotId = _lrepo.GetSnapshotId();
            generate_hello_msg(snapshotId, sequentialId);
        }

        void ISignalsManagerEx.Idle(SignalsManagerEx asFriend)
        {
            _messanger.Idle();
        }

        #endregion
    }
}