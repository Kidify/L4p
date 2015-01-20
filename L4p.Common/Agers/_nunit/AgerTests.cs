using System;
using NUnit.Framework;
using L4p.Common.Extensions;
using L4p.Common.Helpers;
using L4p.Common.Timelines;

namespace L4p.Common.Agers._nunit
{
    [TestFixture]
    class AgerTests
    {
        class Item
        {
            public DateTime Age { get; set; }
        }

        [Test]
        public void When_ufx_quotes_are_aged_IntegrationTest()
        {
            var ager = Ager<Item>.New(x => x.Age);

            Item item = null, item2 = null;

            Timeline.New().Setup()
                .Now(at => Assert.That(ager.GetExpiredItem(at, 10.Milliseconds()), Is.Null))
                .Then(at => item = new Item{Age = at})
                .Now(at => ager.Add(item))
                .Wait(500.Milliseconds())
                .Now(at => Assert.That(ager.GetExpiredItem(at, 800.Milliseconds()), Is.Null))
                .Then(at => item2 = new Item {Age = at})
                .Now(at => ager.Add(item2))
                .Then(at => Assert.That(ager.GetExpiredItem(at, 200.Milliseconds()), Is.SameAs(item)))
                .Now(at => Assert.That(ager.GetExpiredItem(at, 200.Milliseconds()), Is.Null))
                .Wait(500.Milliseconds())
                .Then(at => Assert.That(ager.GetExpiredItem(at, 200.Milliseconds()), Is.SameAs(item2)))
                .Now(at => Assert.That(ager.GetExpiredItem(at, 200.Milliseconds()), Is.Null))
                .Run();
        }
    }
}