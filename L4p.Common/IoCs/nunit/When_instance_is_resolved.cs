using NUnit.Framework;
using L4p.Common.NUnits;

namespace L4p.Common.IoCs.nunit
{
    [TestFixture]
    class When_instance_is_resolved
    {
        [Test]
        public void As_parameterless_call_It_should_be_returned()
        {
            var ioc = IoC.New();
            var obj = new SomeObject();

            ioc.RegisterInstance<ISomeInterface>(obj);
            Assert.That(ioc.Resolve<ISomeInterface>(), Is.SameAs(obj));
        }

        [Test]
        public void As_parameterless_call_And_nothing_is_there_It_should_throw()
        {
            var ioc = IoC.New();
            var obj = new SomeObject();

            ioc.RegisterInstance<ISomeInterface>(obj);

            AssertIt.Fails(() => ioc.Resolve<ISomeInterface2>());
        }
    }
}