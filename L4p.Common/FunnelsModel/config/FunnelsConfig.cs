using System;
using L4p.Common.Extensions;

namespace L4p.Common.FunnelsModel.config
{
    public class FunnelsConfig
    {
        public int Port { get; set; }
        public string ResolvingHost { get; set; }
        public string ResolvingAt { get; set; }
        public string ShopUri { get; set; }
        public string AgentUri { get; set; }
    
        public HubConfig Hub { get; set; }
        public ClientConfig Client { get; set; }

        public class HubConfig
        {
            
        }

        public class ClientConfig
        {
            public int SinkThreadsCount { get; set; }

            public TimeSpan FunnelStoreTtlSpan { get; set; }
            public TimeSpan CleanDeadFunnelsSpan { get; set; }
        }
    }
}