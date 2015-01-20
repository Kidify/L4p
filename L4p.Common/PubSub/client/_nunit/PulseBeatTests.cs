using System.Threading;
using NUnit.Framework;

namespace L4p.Common.PubSub.client._nunit
{
    [TestFixture, Explicit]
    class PulseBeatTests
    {
        [Test]
        public void soon_test()
        {
            int count = 0;

            var timer = new Timer(obj => ++count);
            timer.Change(300, 1000);

            for (int i = 50; i-- > 0;)
            {
                Thread.Sleep(100);
                timer.Change(300, 1000);
            }

            timer.Dispose();
            Assert.That(count, Is.EqualTo(0));
        }
    }
}