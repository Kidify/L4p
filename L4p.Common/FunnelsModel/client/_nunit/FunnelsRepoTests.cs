using System;
using NUnit.Framework;
using L4p.Common.Extensions;
using L4p.Common.Helpers;
using L4p.Common.Loggers;

namespace L4p.Common.FunnelsModel.client._nunit
{
    [TestFixture]
    class FunnelsRepoTests
    {
        [Test]
        public void add_remove_clean_sequence()
        {
            var funnelId = "a_funnel";
            var storeId = Guid.NewGuid();

            var repo = FunnelsRepo.New();
            var store = FunnelStore.New(storeId, Null.Shop, LogFile.Null);

            repo.Add(funnelId, store);
            Assert.That(repo.GetByFunnelId(funnelId), Is.SameAs(store));

            var now = DateTime.UtcNow;

            repo.Remove(store);
            store.DeadAt = now - 2.Seconds();

            Assert.That(repo.PopRemovedStores(now, 1.Seconds()), Is.Empty);

            Assert.That(repo.PopRemovedStores(now, 3.Seconds()), Is.Not.Empty);
            Assert.That(repo.PopRemovedStores(now, 3.Seconds()), Is.Empty);
        }
    }
}