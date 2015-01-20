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
    class Signals_integration_tests
    {
        class Ping
        {
            public int No { get; set; }
        }

        class Pong
        {
            public int No { get; set; }
        }

        [Test]
        public void OffTopicMsg_should_be_filtered()
        {
            var hub = SignalsHub.New();
            var signals = SignalsManagerEx.New();
            var signals2 = SignalsManagerEx.New();

            hub.Start();
            signals.StartAgent();
            signals2.StartAgent();

            Pong pong = null;

            signals.SubscribeTo<Pong>(msg => pong = msg);

            Thread.Sleep(10.Seconds());

            signals2.Publish(new Ping { No = 321 });

            Thread.Sleep(2.Seconds());

            signals2.Publish(new Ping { No = 333 });

            Console.WriteLine("Hub: {0}", hub.Dump().ToJson());
            Console.WriteLine("Signals: {0}", signals.Dump().ToJson());
            Console.WriteLine("Signals2: {0}", signals.Dump().ToJson());

            Thread.Sleep(2.Seconds());

            signals.StopAgent();
            signals2.StopAgent();
            hub.Stop();

            Assert.Pass();
        }

        [Test]
        public void MsgCanBeFiltered_test()
        {
            var hub = SignalsHub.New();
            var signals = SignalsManagerEx.New();
            var signals2 = SignalsManagerEx.New();

            hub.Start();
            signals.StartAgent();
            signals2.StartAgent();

            Thread.Sleep(10.Seconds());

            Ping ping = null;
            Pong pong = null;

            signals.SubscribeTo<Pong>(msg => pong = msg, msg => msg.No == 321);
            signals2.SubscribeTo<Ping>(msg => ping = msg, msg => msg.No == 123);

            Thread.Sleep(2.Seconds());

            signals.Publish(new Ping { No = 123 });
            signals2.Publish(new Pong { No = 321 });

            Thread.Sleep(2.Seconds());

            signals.Publish(new Ping { No = 111 });
            signals2.Publish(new Pong { No = 333 });

            Thread.Sleep(10.Seconds());

            Console.WriteLine("Hub: {0}", hub.Dump().ToJson());
            Console.WriteLine("Signals: {0}", signals.Dump().ToJson());
            Console.WriteLine("Signals2: {0}", signals2.Dump().ToJson());

            signals.StopAgent();
            signals2.StopAgent();

            Thread.Sleep(2.Seconds());

            hub.Stop();

            Assert.That(ping.No, Is.EqualTo(123));
            Assert.That(pong.No, Is.EqualTo(321));
        }

        [Test]
        public void Pusblisher_can_filter_message_test()
        {
            var hub = SignalsHub.New();
            var signals = SignalsManagerEx.New();
            var signals2 = SignalsManagerEx.New();

            hub.Start();
            signals.StartAgent();
            signals2.StartAgent();

            Thread.Sleep(10.Seconds());

            Ping ping = null;
            var matchedId = 123;

            signals2.SubscribeTo<Ping>(
                msg => ping = msg, 
                msg => msg.No == matchedId);

            Thread.Sleep(2.Seconds());

            signals.Publish(new Ping { No = matchedId });
            Thread.Sleep(2.Seconds());

            signals.Publish(new Ping { No = 111 });
            Thread.Sleep(2.Seconds());

            signals.Publish(new Ping { No = 222 });
            Thread.Sleep(2.Seconds());

            Console.WriteLine("Hub: {0}", hub.Dump().ToJson());
            Console.WriteLine("Signals: {0}", signals.Dump().ToJson());
            Console.WriteLine("Signals2: {0}", signals2.Dump().ToJson());

            signals.StopAgent();
            signals2.StopAgent();

            Thread.Sleep(2.Seconds());

            hub.Stop();

            Assert.That(ping.No, Is.EqualTo(matchedId));
        }
    }
}