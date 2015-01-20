using NUnit.Framework;
using L4p.Common.NUnits;

namespace L4p.Common.IoCs.nunit
{
    [TestFixture]
    class When_instance_is_registered
    {
        [Test]
        public void Null_reference_is_not_allowed()
        {
            var ioc = IoC.New();
            AssertIt.Fails(() => ioc.RegisterInstance<ISomeInterface>(null));
        }

        [Test]
        public void It_should_be_used_regardless_local_parameters()
        {
            var ioc = IoC.New();

            var obj = new SomeObject();
            ioc.RegisterInstance<ISomeInterface>(obj);

            Assert.That(ioc.Resolve<ISomeInterface>(), Is.SameAs(obj));
            Assert.That(ioc.Resolve<ISomeInterface>(() => null), Is.SameAs(obj));
        }

        [Test]
        public void It_should_be_of_a_specified_interface()
        {
            var ioc = IoC.New();
            AssertIt.Fails(() => ioc.RegisterInstance<ISomeInterface>(new object()));
        }

        [Test]
        public void It_is_wrong_to_register_others()
        {
            var ioc = IoC.New();

            ioc.RegisterInstance<ISomeInterface>(new SomeObject());

            AssertIt.Fails(() => ioc.RegisterInstance<ISomeInterface>(new SomeObject()));
            AssertIt.Fails(() => ioc.RegisterFactory<ISomeInterface>(() => new SomeObject()));
        }

        [Test]
        public void Same_instance_may_be_registared_twice()
        {
            var ioc = IoC.New();
            var obj = new SomeObject();

            ioc.RegisterInstance<ISomeInterface>(obj);
            AssertIt.Succeeds(() => ioc.RegisterInstance<ISomeInterface>(obj));
        }
    }
}