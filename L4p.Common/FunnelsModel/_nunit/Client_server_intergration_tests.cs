using System;
using System.Threading;
using L4p.Common.FunnelsModel.config;
using NUnit.Framework;
using L4p.Common.FunnelsModel.hub;
using L4p.Common.IoCs;
using L4p.Common.Loggers;

namespace L4p.Common.FunnelsModel._nunit
{
    [TestFixture]
    class Client_server_intergration_tests
    {
        [Test]
        public void Push_Read_Modify_Read()
        {
            var log = LogFile.New("funnels.log");
            var ioc = IoC.New(log);

            var funnels = FunnelsManager.New().Start();
            var funnels2 = FunnelsManager.New().Start();

            var resolver = FunnelsResolver.New(ioc);
            var shop = FunnelsShop.New(ioc, new FunnelsConfig());

            using (resolver.AsServiceAt(log, "net.tcp://localhost:1978/FunnelsHub"))
            using (shop.AsService(log))
            {
                resolver.RegisterShop(shop.GetInfo());

                {
                    var ss = new SomeStruct { Id = 1234, Comment = "my comment" };
                    var funnel = funnels.GetFunnelBy(MyTag.Tag1);
                    funnel.Publish(ss);
                }

                Thread.Sleep(200);

                {
                    var funnel = funnels2.GetFunnelBy(MyTag.Tag1);
                    var ss = funnel.Get<SomeStruct>();

                    Assert.That(ss, Is.Not.Null);
                    Assert.That(ss.Id, Is.EqualTo(1234));

                    ss.Id = 4321;
                    funnel.Publish(ss);
                }

                Thread.Sleep(200);

                {
                    var funnel = funnels.GetFunnelBy(MyTag.Tag1);
                    var ss = funnel.Get<SomeStruct>();

                    Assert.That(ss.Id, Is.EqualTo(4321));
                }
            }

            funnels.Stop();
            funnels2.Stop();
        }
    }
}