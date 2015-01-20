using System;
using L4p.Common.Extensions;

namespace L4p.Common.PubSub
{
    public class SignalsConfig
    {
        public int Port { get; set; }
        public string HubHost { get; set; }

        public string HubUri { get; set; }
        public string AgentUri { get; set; }

        public HubArgs Hub { get; set; }
        public ClientArgs Client { get; set; }

        public TimeSpan ThrottledLogTtl { get; set; }
        public bool TraceOn { get; set; }

        public class HubArgs
        {
            public TimeSpan IdleSpan { get; set; }
            public TimeSpan InactiveAgentTtl { get; set; }
            public TimeSpan DeadAgentTtl { get; set; }
            public TimeSpan DumpToLogPeriod { get; set; }
        }

        public class ClientArgs
        {
            public TimeSpan HelloMsgPeriod { get; set; }
            public TimeSpan HelloMsgOnChangeSpan { get; set; }
            public TimeSpan DumpToLogPeriod { get; set; }
            public TimeSpan IdleSpan { get; set; }
            public TimeSpan HeartbeatSpan { get; set; }
            public int PublishRetryCount { get; set; }
        }

        public SignalsConfig()
        {
            Port = 1978;
            HubHost = "localhost";
            HubUri = "net.tcp://{0}:{1}/SignalsHub";
            AgentUri = "net.tcp://{host}:{port}/SignalsHub/agent/{process}/{guid}";
            ThrottledLogTtl = 15.Seconds();
            TraceOn = false;
            Hub = new HubArgs {
                IdleSpan = 5.Seconds(),
                InactiveAgentTtl = 10.Seconds(),
                DeadAgentTtl = 30.Seconds(),
                DumpToLogPeriod = 1.Hours()
            };
            Client = new ClientArgs {
                HelloMsgPeriod = 2.Seconds(),
                HelloMsgOnChangeSpan = 300.Milliseconds(),
                DumpToLogPeriod = 1.Hours(),
                IdleSpan = 20.Seconds(),
                HeartbeatSpan = 1.Minutes(),
                PublishRetryCount = 2
            };
        }
    }
}