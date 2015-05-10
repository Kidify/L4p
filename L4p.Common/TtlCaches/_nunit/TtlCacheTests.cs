using System;
using System.Threading;
using L4p.Common.Extensions;
using NUnit.Framework;

namespace L4p.Common.TtlCaches._nunit
{
    [TestFixture]
    class When_items_are_added
    {
        class MyFacet {}
        class MyBody {}

        private MyBody[] shoot_dead_bodies(ITtlCache<MyFacet, MyBody> cache)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Thread.Sleep(10.Milliseconds());

            return
                cache.GetDeadBodies(1.Milliseconds());
        }

        [Test]
        public void No_shots_no_bodies()
        {
            var cache = TtlCache<MyFacet, MyBody>.New();

            var body = new MyBody();
            var facet = new MyFacet();

            cache.Store(facet, body);

            var bodies = cache.GetDeadBodies(1.Milliseconds());
            Assert.Null(bodies);
        }

        [Test]
        public void Item_should_be_removed_after_its_deatch()
        {
            var cache = TtlCache<MyFacet, MyBody>.New();

            var body = new MyBody();
            var facet = new MyFacet();

            cache.Store(facet, body);

            facet = null;
            var bodies = shoot_dead_bodies(cache);

            Assert.That(bodies[0], Is.SameAs(body));
        }

        [Test]
        public void Double_reference_Item_should_be_removed_after_its_deatch()
        {
            var cache = TtlCache<MyFacet, MyBody>.New();

            var body = new MyBody();

            var facet = new MyFacet();
            var facet2 = new MyFacet();

            cache.Store(facet, body);
            cache.Store(facet2, body);

            cache.Store(facet, body);     // should be ignored

            facet = null;
            var bodies = shoot_dead_bodies(cache);

            Assert.Null(bodies);

            facet2 = null;
            bodies = shoot_dead_bodies(cache);

            Assert.That(bodies[0], Is.SameAs(body));
        }
    }
}