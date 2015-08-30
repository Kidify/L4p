using System;
using System.Diagnostics;
using System.Dynamic;
using System.Threading;
using L4p.Common.DumpToLogs;
using L4p.Common.Helpers;
using L4p.Common.Wcf;

namespace L4p.Common.PubSub.client.Io
{
    interface IAgentWriter : IHaveDump
    {
        string AgentUri { get; }
        TimeSpan NonActiveSpan { get; }

        void Publish(comm.PublishMsg msg);
        void FilterTopicMsgs(comm.TopicFilterMsg msg);
        void Heartbeat(comm.HeartbeatMsg msg);
        void Close();
    }

    class AgentWriter
        : WcfProxy<comm.IAgentAsyncWriter>, IAgentWriter
    {
        #region counters

        class Counters
        {
            public long PublishCalled;
            public long FilterTopicMsgSent;
            public long HeartbeatMsgSent;
            public long StartIoFailed;
            public long EndIoFailed;
        }

        #endregion

        #region members

        private readonly Counters _counters;
        private readonly string _agentUri;
        private readonly int _sequentialId;
        private readonly IMessengerEngine _messenger;
        private readonly Stopwatch _tmFromLastMsg;

        #endregion

        #region construction

        public static IAgentWriter New(int sequentialId, string agentUri, IMessengerEngine messenger)
        {
            return
                new AgentWriter(sequentialId, agentUri, messenger);
        }

        private AgentWriter(int sequentialId, string agentUri, IMessengerEngine messenger)
            : base(agentUri)
        {
            _counters = new Counters();
            _agentUri = agentUri;
            _sequentialId = sequentialId;
            _messenger = messenger;
            _tmFromLastMsg = Stopwatch.StartNew();
        }

        #endregion

        #region private

        private void start_io(comm.IoMsg msg, Action startIo)
        {
            try
            {
                startIo();
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref _counters.StartIoFailed);
                _messenger.IoFailed(this, msg, ex, "start_io");
            }
        }

        private void end_io(comm.IoMsg msg, Action endIo)
        {
            try
            {
                endIo();
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref _counters.EndIoFailed);
                _messenger.IoFailed(this, msg, ex, "end_io");
            }
        }

        private void make_io(comm.IoMsg msg,
            Action<AsyncCallback> io, Action<IAsyncResult> onComplete)
        {
            Validate.NotNull(msg);

            msg.Guid = Guid.NewGuid();
            msg.AgentUri = _agentUri;

            AsyncCallback cb = ar =>
                end_io(msg, () => onComplete(ar));

            start_io(msg, () => io(cb));

            _tmFromLastMsg.Restart();
        }

        #endregion

        #region interface

        string IAgentWriter.AgentUri
        {
            get { return _agentUri; }
        }

        TimeSpan IAgentWriter.NonActiveSpan
        {
            get { return _tmFromLastMsg.Elapsed; }
        }

        void IAgentWriter.Publish(comm.PublishMsg msg)
        {
            Interlocked.Increment(ref _counters.PublishCalled);

            make_io(msg,
                cb => Channel.BeginPublish(msg, cb, null),
                ar => Channel.EndPublish(ar));
        }

        void IAgentWriter.FilterTopicMsgs(comm.TopicFilterMsg msg)
        {
            Interlocked.Increment(ref _counters.FilterTopicMsgSent);

            make_io(msg,
                cb => Channel.BeginFilterTopicMsgs(msg, cb, null),
                ar => Channel.EndFilterTopicMsgs(ar));
        }

        void IAgentWriter.Heartbeat(comm.HeartbeatMsg msg)
        {
            Interlocked.Increment(ref _counters.HeartbeatMsgSent);

            make_io(msg,
                cb => Channel.BeginHeartbeat(cb, null),
                ar => Channel.EndHeartbeat(ar));
        }

        void IAgentWriter.Close()
        {
            IWcfProxy self = this;
            self.Close();
        }

        ExpandoObject IHaveDump.Dump(dynamic root)
        {
            if (root == null)
                root = new ExpandoObject();

            root.AgentUri = _agentUri;
            root.SequentialId = _sequentialId;
            root.NonActiveSpan = _tmFromLastMsg.Elapsed;
            root.Counters = _counters;

            return root;
        }

       #endregion
    }
}