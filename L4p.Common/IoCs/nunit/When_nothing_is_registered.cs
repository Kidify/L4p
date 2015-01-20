using NUnit.Framework;
using L4p.Common.NUnits;

namespace L4p.Common.IoCs.nunit
{
    [TestFixture]
    class When_nothing_is_registered
    {
        [Test]
        public void And_resolving_It_should_throw()
        {
            var ioc = IoC.New();
            AssertIt.Fails(() => ioc.Resolve<ISomeInterface>());
        }

        [Test]
        public void It_should_use_local_factory()
        {
            var ioc = IoC.New();

            var obj = new SomeObject();

            Assert.That(ioc.Resolve<ISomeInterface>(() => obj), Is.SameAs(obj));
        }
    }
}