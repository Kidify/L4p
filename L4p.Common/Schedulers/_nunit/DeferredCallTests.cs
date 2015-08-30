using System.Threading;
using L4p.Common.Extensions;
using NUnit.Framework;

namespace L4p.Common.Schedulers._nunit
{
    [TestFixture]
    class DeferredCallTests
    {
        [Test, Explicit]
        public void should_be_delayed()
        {
            bool done = false;

            DeferredCall.Start(200.Milliseconds(), () => done = true);
            Assert.That(done, Is.False);

            Thread.Sleep(50);
            Assert.That(done, Is.False);

            Thread.Sleep(300);
            Assert.That(done, Is.True);
        }
    }
}