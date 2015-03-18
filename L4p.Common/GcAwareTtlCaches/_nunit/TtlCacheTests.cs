using System;
using System.Threading;
using NUnit.Framework;
using L4p.Common.Extensions;

namespace L4p.Common.GcAwareTtlCaches._nunit
{
    [TestFixture]
    class When_items_are_added
    {
        class MyFacet {}
        class MyInstance {}

        private MyInstance[] shoot_dead_bodies(ITtlCache<MyFacet, MyInstance> cache)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Thread.Sleep(10.Milliseconds());

            return
                cache.GetDeadInstances(1.Milliseconds());
        }

        [Test]
        public void No_shots_no_bodies()
        {
            var cache = TtlCache<MyFacet, MyInstance>.New();

            var instance = new MyInstance();
            var facet = new MyFacet();

            cache.AddInstance(facet, instance);

            var bodies = cache.GetDeadInstances(1.Milliseconds());
            Assert.Null(bodies);
        }

        [Test]
        public void Item_should_be_removed_after_its_deatch()
        {
            var cache = TtlCache<MyFacet, MyInstance>.New();

            var instance = new MyInstance();
            var facet = new MyFacet();

            cache.AddInstance(facet, instance);

            facet = null;
            var bodies = shoot_dead_bodies(cache);

            Assert.That(bodies[0], Is.SameAs(instance));
        }

        [Test]
        public void Double_reference_Item_should_be_removed_after_its_deatch()
        {
            var cache = TtlCache<MyFacet, MyInstance>.New();

            var instance = new MyInstance();

            var facet = new MyFacet();
            var facet2 = new MyFacet();

            cache.AddInstance(facet, instance);
            cache.AddInstance(facet2, instance);

            cache.AddInstance(facet, instance);     // should be ignored

            facet = null;
            var bodies = shoot_dead_bodies(cache);

            Assert.Null(bodies);

            facet2 = null;
            bodies = shoot_dead_bodies(cache);

            Assert.That(bodies[0], Is.SameAs(instance));
        }
    }
}