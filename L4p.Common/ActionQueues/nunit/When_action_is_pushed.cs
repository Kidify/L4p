using NUnit.Framework;

namespace L4p.Common.ActionQueues.nunit
{
    [TestFixture]
    class When_action_is_pushed
    {
        [Test]
        public void It_should_be_counted()
        {
            var que = ActionQueue.New();
            que.Push(() => { });

            Assert.That(que.Count == 1);
        }

        public void It_should_be_popped()
        {
            var que = ActionQueue.New();
            que.Push(() => { });

            Assert.That(que.Pop(), Is.Not.Null);
        }

        public void It_returns_action_queue_length()
        {
            Assert.Fail();
        }
    }
}