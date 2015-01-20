using NUnit.Framework;
using L4p.Common.Loggers;

namespace L4p.Common.CountersAccumulators.nunit
{
    [TestFixture]
    class When_counters_are_accumulated
    {
        [Test]
        public void They_should_be_summed()
        {
            var accumulator = CountersAccumulator.New(LogFile.Console);

            var counters = new SomeCounters
                {
                    One = 11,
                    Two = 22,
                    Three = 0.33,
                    Inner = new SomeCounters.InnerCounters
                        {
                            InnerOne = 1,
                            InnerDouble = 0.123,
                        }
                };

            var counters2 = new SomeCounters
                {
                    One = 11,
                    Two = 20,
                    Three = 0.44,
                    Inner = new SomeCounters.InnerCounters
                        {
                            InnerOne = 2,
                            InnerDouble = 0.321,
                        }
                };

            accumulator.Sum(counters);
            accumulator.Sum(counters2);

            var result = accumulator.GetSum<SomeCounters>();

            Assert.That(result.One, Is.EqualTo(22));
            Assert.That(result.Two, Is.EqualTo(42));
            Assert.That(result.Three, Is.EqualTo(0.77));
            Assert.That(result.Inner.InnerOne, Is.EqualTo(3));
            Assert.That(result.Inner.InnerDouble, Is.EqualTo(0.444));
        }

        [Test]
        public void They_should_be_diffirintiated_by_type()
        {
            var accumulator = CountersAccumulator.New(LogFile.Console);

            accumulator.Sum(new SomeCounters { One = 123 });
            accumulator.Sum(new OtherCounters { One = 321 });

            Assert.That(accumulator.GetSum<SomeCounters>().One, Is.EqualTo(123));
            Assert.That(accumulator.GetSum<OtherCounters>().One, Is.EqualTo(321));
        }
    }
}