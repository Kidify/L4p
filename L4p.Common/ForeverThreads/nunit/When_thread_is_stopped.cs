using NUnit.Framework;
using L4p.Common.Loggers;

namespace L4p.Common.ForeverThreads.nunit
{
    [TestFixture]
    class When_thread_is_stopped
    {
        [Test]
        public void It_should_be_stopped_within_timeout()
        {
            var thr = ForeverThread.New(() => { });
            Assert.That(() => thr.Stop(), Throws.Nothing.After(1000, 100));
        }

        [Test]
        public void Its_should_be_reflected()
        {
            var thr = ForeverThread.New(() => { });
            thr.Stop();

            Assert.That(thr.StopRequestIsPosted(), Is.True);
        }

        [Test]
        public void It_should_throw_after_timeout()
        {
            var thr = ForeverThread.New(() => { }, LogFile.Console, new ForeverThreadConfig { StopTimeout = 0 });
            Assert.That(() => thr.Stop(), Throws.InstanceOf<ForeverThreadException>().After(1000, 100));
        }
    }
}