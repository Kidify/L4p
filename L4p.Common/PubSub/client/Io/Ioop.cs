using L4p.Common.PubSub.comm;

namespace L4p.Common.PubSub.client.Io
{
    class Ioop
    {
        public PublishMsg Msg { get; set; }
        public int Retries { get; set; }
        public IAgentWriter Proxy { get; set; }
    }
}