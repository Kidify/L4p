using System.ServiceModel;

namespace L4p.Common.PubSub.hub.wcf
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple,
        IncludeExceptionDetailInFaults = true
        )]
    class SignalsHub : comm.ISignalsHub
    {
        #region members

        private readonly ISignalsHub _target;

        #endregion

        #region construction

        public static comm.ISignalsHub New(ISignalsHub target)
        {
            return
                new SignalsHub(target);
        }

        private SignalsHub(ISignalsHub target)
        {
            _target = target;
        }

        #endregion

        #region interface

        public void Dispose()
        {
            throw
                new ShouldNotBeCalledException();
        }

        void comm.ISignalsHub.Hello(comm.HelloMsg msg)
        {
            _target.Hello(msg);
        }

        void comm.ISignalsHub.Goodbye(string agentUri)
        {
            _target.Goodbye(agentUri);
        }

        #endregion
    }
}