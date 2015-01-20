using L4p.Common.PubSub.comm;
using L4p.Common.Wcf;

namespace L4p.Common.PubSub.client.wcf
{
    class SignalsHub
        : WcfProxy<ISignalsHub>, ISignalsHub
    {
        public static ISignalsHub New(string uri)
        {
            return new SignalsHub(uri);
        }

        private SignalsHub(string uri)
            : base(uri)
        { }

        void ISignalsHub.Hello(HelloMsg msg)
        {
            Channel.Hello(msg);
        }

        void ISignalsHub.Goodbye(string agentUri)
        {
            Channel.Goodbye(agentUri);
        }
    }
}