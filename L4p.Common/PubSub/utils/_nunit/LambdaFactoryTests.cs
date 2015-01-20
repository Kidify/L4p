using NUnit.Framework;

namespace L4p.Common.PubSub.utils._nunit
{
    [TestFixture]
    class LambdaFactoryTests
    {
        private readonly int _memberOfThis = 0;

        [Test]
        public void static_filter_test()
        {
            var lambda = FiltersEngine.New();

            var info = lambda.MakeFilterInfo<SomeMsg>(msg => msg.Id == 111);

            var filters = lambda.BuildFilters(new[] {info});

            Assert.That(filters, Is.Not.Empty);
            Assert.That(filters.Length, Is.EqualTo(1));

            var filter = filters[0];
            bool matched = filter(new SomeMsg {Id = 111});
            bool notMatched = !filter(new SomeMsg {Id = 222});

            Assert.That(matched, Is.True);
            Assert.That(notMatched, Is.True);
        }

        [Test]
        public void instance_filter_test()
        {
            var lambda = FiltersEngine.New();

            var id = 111;
            var info = lambda.MakeFilterInfo<SomeMsg>(msg => msg.Id == id);

            var filters = lambda.BuildFilters(new[] { info });

            Assert.That(filters, Is.Not.Empty);
            Assert.That(filters.Length, Is.EqualTo(1));

            var filter = filters[0];
            bool matched = filter(new SomeMsg { Id = 111 });
            bool notMatched = !filter(new SomeMsg { Id = 222 });

            Assert.That(matched, Is.True);
            Assert.That(notMatched, Is.True);
        }

        class MyMsg
        {
            public int IntFld { get; set; }
        }

        [Test]
        public void detect_non_transferable_filters()
        {
            var lambda = FiltersEngine.New();

            // constants only - lambda is a static method
            {
                bool isTransferable = lambda.FilterCanBeTransferred<MyMsg>(
                    msg => msg.IntFld == 123);

                Assert.That(isTransferable, Is.True);
            }

            // locals only - lambda is member of a generated class
            {
                var local = _memberOfThis;

                bool isTransferable = lambda.FilterCanBeTransferred<MyMsg>(
                    msg => msg.IntFld == local);

                Assert.That(isTransferable, Is.True);
            }

            // locals and members - lambda is member of a generated class
            {
                var local = _memberOfThis;

                bool isTransferable = lambda.FilterCanBeTransferred<MyMsg>(
                    msg => msg.IntFld == local && msg.IntFld == _memberOfThis);

                Assert.That(isTransferable, Is.True);
            }

            // members only - lambda is member of the LambdaFactoryTests
            {
                bool isTransferable = lambda.FilterCanBeTransferred<MyMsg>(
                    msg => msg.IntFld == _memberOfThis);

                Assert.That(isTransferable, Is.False);
            }
        }
    }
}