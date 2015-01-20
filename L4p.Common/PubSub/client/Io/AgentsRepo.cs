using System;
using System.Collections.Generic;
using System.Linq;
using L4p.Common.Concerns;

namespace L4p.Common.PubSub.client.Io
{
    interface IAgentsRepo
    {
        IAgentWriter GetAgentProxy(string agentUri);
        IAgentWriter SetAgentProxy(string agentUri, IAgentWriter proxy);
        IAgentWriter RemoveAgent(string agentUri);
        IAgentWriter RemoveAgent(IAgentWriter proxy);
        IAgentWriter[] GetAll();
    }

    class AgentsRepo : IAgentsRepo
    {
        #region members

        private readonly Dictionary<string, IAgentWriter> _proxies;

        #endregion

        #region construction

        public static IAgentsRepo New()
        {
            return
                new AgentsRepo();
        }

        public static IAgentsRepo NewSync()
        {
            return
                MReadSWriteAgentsRepo.New(
                new AgentsRepo());
        }

        private AgentsRepo()
        {
            _proxies = new Dictionary<string, IAgentWriter>();
        }

        #endregion

        #region private
        #endregion

        #region interface

        IAgentWriter IAgentsRepo.GetAgentProxy(string agentUri)
        {
            IAgentWriter proxy;
            _proxies.TryGetValue(agentUri, out proxy);

            return proxy;
        }

        IAgentWriter IAgentsRepo.SetAgentProxy(string agentUri, IAgentWriter proxy)
        {
            IAgentWriter prev;

            _proxies.TryGetValue(agentUri, out prev);
            _proxies[agentUri] = proxy;

            return prev;
        }

        IAgentWriter IAgentsRepo.RemoveAgent(string agentUri)
        {
            IAgentWriter proxy;
            _proxies.TryGetValue(agentUri, out proxy);

            _proxies.Remove(agentUri);

            return proxy;
        }

        IAgentWriter IAgentsRepo.RemoveAgent(IAgentWriter proxy)
        {
            IAgentWriter current;
            _proxies.TryGetValue(proxy.AgentUri, out current);

            if (!ReferenceEquals(proxy, current))
                return null;

            _proxies.Remove(proxy.AgentUri);

            return proxy;
        }

        public IAgentWriter[] GetAll()
        {
            return
                _proxies.Values.ToArray();
        }

        #endregion
    }

    class MReadSWriteAgentsRepo : ManyReadersSingleWriter<IAgentsRepo>, IAgentsRepo
    {
        public static IAgentsRepo New(IAgentsRepo impl) { return new MReadSWriteAgentsRepo(impl); }
        private MReadSWriteAgentsRepo(IAgentsRepo impl) : base(impl) { }

        IAgentWriter IAgentsRepo.GetAgentProxy(string agentUri) { return using_read_lock(() => _impl.GetAgentProxy(agentUri)); }
        IAgentWriter IAgentsRepo.SetAgentProxy(string agentUri, IAgentWriter proxy) { return using_write_lock(() => _impl.SetAgentProxy(agentUri, proxy)); }
        IAgentWriter IAgentsRepo.RemoveAgent(string agentUri) { return using_write_lock(() => _impl.RemoveAgent(agentUri)); }
        IAgentWriter IAgentsRepo.RemoveAgent(IAgentWriter proxy) { return using_write_lock(() => _impl.RemoveAgent(proxy)); }
        IAgentWriter[] IAgentsRepo.GetAll() { return using_read_lock(() => _impl.GetAll()); }
   }
}