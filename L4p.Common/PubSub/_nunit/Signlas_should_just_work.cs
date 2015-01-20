using System;
using System.Threading;
using NUnit.Framework;
using L4p.Common.Json;
using L4p.Common.PubSub.client;
using L4p.Common.PubSub.hub;

namespace L4p.Common.PubSub._nunit
{
    [TestFixture]
    class Signlas_should_just_work
    {
        class SomeMsg
        {
            public int Int { get; set; }
            public string Str { get; set; }
        }

        [Test]
        public void local_publish_subscriber()
        {
            var msg = new SomeMsg {Int = 123, Str = "I'm here"};
            SomeMsg msg2 = null;

            Signals.SubscribeTo<SomeMsg>(x => msg2 = x);
            Signals.Publish(msg);

            Thread.Sleep(200);

            Assert.That(msg2, Is.Not.Null);
            Assert.That(msg2.ToJson(), Is.EqualTo(msg.ToJson()));
        }

        [Test, Explicit]
        public void remote_publish_subscriber()
        {
            var hub = SignalsHub.New();
            var signals = SignalsManagerEx.New();
            var signals2 = SignalsManagerEx.New();

            hub.Start();

            signals.StartAgent();
            signals2.StartAgent();

            Thread.Sleep(5000);

            var msg = new SomeMsg { Int = 123, Str = "I'm here" };
            SomeMsg msg2 = null;

            signals2.SubscribeTo<SomeMsg>(x => msg2 = x);
            Thread.Sleep(2000);

            signals.Publish(msg);
            Thread.Sleep(2000);

            Assert.That(msg2, Is.Not.Null);
            Assert.That(msg2, Is.Not.SameAs(msg));
            Assert.That(msg2.ToJson(), Is.EqualTo(msg.ToJson()));

            Console.WriteLine("Hub: {0}", hub.Dump().ToJson());
            Console.WriteLine("Signals: {0}", signals.Dump().ToJson());
            Console.WriteLine("Signals2: {0}", signals2.Dump().ToJson());

            signals.StopAgent();
            signals2.StopAgent();

            Thread.Sleep(2000);

            hub.Stop();
        }
    }
}