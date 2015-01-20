using NUnit.Framework;
using L4p.Common.Loggers;

namespace L4p.Common.ForeverThreads.nunit
{
    [TestFixture]
    class When_thread_is_started
    {
        [Test]
        public void It_should_be_started_within_timeout()
        {
            var thr = ForeverThread.New(() => { });
            Assert.That(() => thr.Start(), Throws.Nothing.After(1000, 100));
        }

        [Test]
        public void It_should_throw_after_timeout()
        {
            var thr = ForeverThread.New(() => { }, LogFile.Console, new ForeverThreadConfig {StartTimeout = 0});
            Assert.That(() => thr.Start(), Throws.InstanceOf<ForeverThreadException>().After(100));
        }

        [Test]
        public void It_should_stop_after_work_is_done()
        {
            var thr = ForeverThread.New(() => { });
            thr.Start();
            Assert.That(() => thr.IsStopped(), Is.True.After(1000, 100));
        }
    }
}