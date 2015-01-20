using System;
using System.Threading;
using NUnit.Framework;

namespace L4p.Common.ActiveObjects.nunit
{
    [TestFixture]
    class When_action_is_performed
    {
        [Test]
        public void It_is_performed_withing_different_thread()
        {
            Thread current = Thread.CurrentThread;
            Thread actual = null;

            IActiveObject ao = new ActiveTest();
            ao.Start();

            ao.PushAction(() => actual = Thread.CurrentThread );
            Thread.Sleep(100);

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.Not.SameAs(current));
        }

        [Test]
        public void Counters_should_be_updated()
        {
            IActiveObject ao = new ActiveTest();
            ao.Start();

            ao.PushAction(() => {});
            Thread.Sleep(100);

            var counters = ao.GetCounters;

            Assert.That(counters.Posted == 1);
            Assert.That(counters.Started == 1);
            Assert.That(counters.Completed == 1);
            Assert.That(counters.Succeeded == 1);
            Assert.That(counters.Failed == 0);

            ao.Stop();
        }

        [Test]
        public void And_falied_Counters_should_be_updated()
        {
            IActiveObject ao = new ActiveTest();
            ao.Start();

            ao.PushAction(() => { throw new NotImplementedException(); });
            Thread.Sleep(100);

            var counters = ao.GetCounters;

            Assert.That(counters.Posted == 1);
            Assert.That(counters.Started == 1);
            Assert.That(counters.Completed == 1);
            Assert.That(counters.Succeeded == 0);
            Assert.That(counters.Failed == 1);

            ao.Stop();
        }
    }
}