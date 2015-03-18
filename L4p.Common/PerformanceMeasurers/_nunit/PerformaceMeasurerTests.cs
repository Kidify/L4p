using System.Threading;
using L4p.Common.Loggers;
using NUnit.Framework;

namespace L4p.Common.PerformanceMeasurers._nunit
{
    [TestFixture]
    class PerformaceMeasurerTests
    {
        private readonly IPerformaceMeasurer _perf = PerformaceMeasurer.New();

        private void foo()
        {
            _perf.TimeOf(LogFile.Console, () => foo());
            Thread.Sleep(97);
        }

        private void boo()
        {
            _perf.TimeOf(LogFile.Console, () => boo());
            Thread.Sleep(123);
        }

        [Test]
        public void take_the_time()
        {
            foo();
            boo();
        }
    }
}