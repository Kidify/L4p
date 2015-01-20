using System;
using System.Threading;
using NUnit.Framework;
using L4p.Common.Extensions;
using L4p.Common.Json;
using L4p.Common.PubSub.client;
using L4p.Common.PubSub.hub;

namespace L4p.Common.PubSub._nunit
{
    [TestFixture, Explicit]
    class When_agent_is_dead
    {
        [Test]
        public void It_should_be_removed()
        {
            var hub = SignalsHub.New();
            var signals = SignalsManagerEx.New();
            var signals2 = SignalsManagerEx.New();

            hub.Start();
            signals.StartAgent();
            signals2.StartAgent();

            Thread.Sleep(5.Seconds());
            signals2.StopAgent();

            Thread.Sleep(40.Seconds());

            Console.WriteLine("Hub: {0}", hub.Dump().ToJson());
            Console.WriteLine("Signals: {0}", signals.Dump().ToJson());

            signals.StopAgent();
            hub.Stop();
        }
    }
}