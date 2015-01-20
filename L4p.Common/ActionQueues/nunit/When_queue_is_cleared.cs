using NUnit.Framework;

namespace L4p.Common.ActionQueues.nunit
{
    [TestFixture]
    class When_queue_is_cleared
    {
        [Test]
        public void It_should_be_empty()
        {
            var que = ActionQueue.New();

            que.Push(() => { });
            que.Clear();

            Assert.That(que.Count == 0);
            Assert.That(que.Pop(), Is.Null);
        }
    }
}