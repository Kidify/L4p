using System.Threading;
using NUnit.Framework;

namespace L4p.Common.ActiveObjects.nunit
{
    [TestFixture]
    class When_action_is_pushed
    {
        [Test]
        public void It_should_be_performed()
        {
            var done = new ManualResetEvent(false);

            IActiveObject ao = new ActiveTest();
            ao.Start();

            ao.PushAction(() => done.Set());

            Assert.That(done.WaitOne(100), Is.True);

            ao.Stop();
        }

        [Test]
        public void Before_object_is_started_It_should_be_performed_after_start()
        {
            var done = new ManualResetEvent(false);

            IActiveObject ao = new ActiveTest();

            ao.PushAction(() => done.Set());
            Assert.That(done.WaitOne(300), Is.False);

            ao.Start();
            Assert.That(done.WaitOne(100), Is.True);

            ao.Stop();
        }

        [Test]
        public void It_should_return_number_of_messages_in_queue()
        {
            Assert.Fail();
        }
    }
}