using NUnit.Framework;
using L4p.Common.NUnits;

namespace L4p.Common.IoCs.nunit
{
    [TestFixture]
    class When_single_instance_is_needed
    {
        [Test]
        public void It_should_be_created_once()
        {
            var ioc = IoC.New();

            var obj = ioc.SingleInstance<ISomeInterface>(() => new SomeObject());
            var obj2 = ioc.SingleInstance<ISomeInterface>(() => new SomeObject());

            Assert.That(obj2, Is.SameAs(obj));
        }
    }

    [TestFixture]
    class When_factory_is_registered
    {
        [Test]
        public void Null_reference_is_not_allowed()
        {
            var ioc = IoC.New();
            AssertIt.Fails(() => ioc.RegisterFactory<ISomeInterface>(null));
        }

        [Test]
        public void It_should_be_used_regardless_local_parameters()
        {
            var ioc = IoC.New();

            var obj = new SomeObject();
            ioc.RegisterFactory<ISomeInterface>(() => obj);

            Assert.That(ioc.Resolve<ISomeInterface>(), Is.SameAs(obj));
            Assert.That(ioc.Resolve<ISomeInterface>(() => null), Is.SameAs(obj));
        }

        [Test]
        public void It_should_be_of_a_specified_interface()
        {
            var ioc = IoC.New();

            ioc.RegisterFactory<ISomeInterface>(() => new object());

            AssertIt.Fails(() => ioc.Resolve<ISomeInterface>());
        }

        [Test]
        public void It_is_wrong_to_register_others()
        {
            var ioc = IoC.New();

            ioc.RegisterFactory<ISomeInterface>(() => new SomeObject());

            AssertIt.Fails(() => ioc.RegisterInstance<ISomeInterface>(new SomeObject()));
            AssertIt.Fails(() => ioc.RegisterFactory<ISomeInterface>(() => new SomeObject()));
        }
    }
}