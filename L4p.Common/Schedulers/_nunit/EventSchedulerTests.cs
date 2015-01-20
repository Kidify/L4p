using System;
using System.Threading;
using NUnit.Framework;
using L4p.Common.Extensions;
using L4p.Common.Helpers;
using L4p.Common.Loggers;

namespace L4p.Common.Schedulers._nunit
{
    [TestFixture]
    class When_event_is_scheduled
    {
        [Test]
        public void Once()
        {
            bool fired = false;

            var scheduler = EventScheduler.New(LogFile.Console);
            scheduler.Start();

            scheduler.FireOnce(1.Milliseconds(), () => fired = true);
            Assert.That(() => fired, Is.True.After(100, 10));

            scheduler.Stop();
        }

        [Test]
        public void Repeat()
        {
            int count = 0;

            var scheduler = EventScheduler.New(LogFile.Console);
            scheduler.Start();

            scheduler.Repeat(1.Milliseconds(), () => count++);
            Assert.That(() => count, Is.GreaterThan(3).After(500, 10));

            scheduler.Stop();
        }

        [Test]
        public void Count()
        {
            int count = 0;

            var scheduler = EventScheduler.New(LogFile.Console);
            scheduler.Start();

            var info = new EventInfo
                {
                    NextShot = DateTime.UtcNow + 20.Milliseconds(),
                    RepeatAfter = 1.Milliseconds(), 
                    Count = 2
                };

            scheduler.Schedule(info, () => count++);
            Assert.That(() => count, Is.EqualTo(2).After(200));

            scheduler.Stop();
        }
    }
}