using NUnit.Framework;

namespace L4p.Common.ActionQueues.nunit
{
    [TestFixture]
    class When_action_is_popped
    {
        [Test]
        public void It_should_work_as_FIFO()
        {
            int order = 0;

            var que = ActionQueue.New();
            que.Push(() => order = 1);
            que.Push(() => order = 2);

            que.Pop()();

            Assert.That(order == 1);
        }

        [Test]
        public void And_queue_is_empty_It_should_return_null()
        {
            var que = ActionQueue.New();
            Assert.That(que.Pop(), Is.Null);
        }

        [Test]
        public void It_is_removed_from_queue()
        {
            var que = ActionQueue.New();

            que.Push(() => { });
            que.Pop();

            Assert.That(que.Count == 0);
        }
    }
}