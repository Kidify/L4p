using System;
using NUnit.Framework;

namespace L4p.Common.CountersAccumulators.nunit
{
    [TestFixture]
    class When_counters_are_formatted
    {
        [Test]
        public void Their_values_should_be_sorted()
        {
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

            string str = CountersHelpers.Format(counters, "-->", 50);
            Console.WriteLine(str);

            Assert.Pass();
        }
    }
}