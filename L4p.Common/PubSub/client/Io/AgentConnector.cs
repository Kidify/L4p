using System.Dynamic;
using System.Threading;
using L4p.Common.DumpToLogs;
using L4p.Common.IoCs;
using L4p.Common.Loggers;

namespace L4p.Common.PubSub.client.Io
{
    interface IAgentConnector : IHaveDump
    {
        IAgentWriter ConnectAgent(string agentUri, IMessangerEngine messanger);
        void DisconnectAgent(IAgentWriter proxy);
    }

    class AgentConnector : IAgentConnector
    {
        #region counters

        class Counters
        {
            public int AgentConnected;
            public int AgentDisconnected;
        }

        #endregion

        #region members

        private readonly Counters _counters;
        private readonly ILogFile _log;
        private int _sequentialId;

        #endregion

        #region construction

        public static IAgentConnector New(IIoC ioc)
        {
            return
                new AgentConnector(ioc);
        }

        private AgentConnector(IIoC ioc)
        {
            _counters = new Counters();
            _log = ioc.Resolve<ILogFile>();
        }

        #endregion

        #region private
        #endregion

        #region interface

        IAgentWriter IAgentConnector.ConnectAgent(string agentUri, IMessangerEngine messanger)
        {
            int sequentialId = Interlocked.Increment(ref _sequentialId);

            var proxy = AgentWriter.New(sequentialId, agentUri, messanger);
            Interlocked.Increment(ref _counters.AgentConnected);

            _log.Info("Agent at '{0}' is connected; sequentialId={1}", agentUri, sequentialId);

            return proxy;
        }

        void IAgentConnector.DisconnectAgent(IAgentWriter proxy)
        {
            proxy.Close();
            Interlocked.Increment(ref _counters.AgentDisconnected);

            _log.Info("Agent at '{0}' is disconnected", proxy.AgentUri);
        }

        ExpandoObject IHaveDump.Dump(dynamic root)
        {
            if (root == null)
                root = new ExpandoObject();

            root.SequentialId = _sequentialId;
            root.Counters = _counters;

            return root;
        }

        #endregion
    }
}