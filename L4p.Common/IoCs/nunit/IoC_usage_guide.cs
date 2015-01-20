using Moq;
using NUnit.Framework;

namespace L4p.Common.IoCs.nunit
{
    interface ISomeInterface {}
    class SomeObject : ISomeInterface {}

    interface ISomeInterface2 { }
    class SomeObject2 : ISomeInterface2 { }

    [TestFixture]
    class When_resolving
    {
        [Test]
        public void One_should_use_local_factories_like_this()
        {
            var ioc = IoC.New();

            var obj = ioc.Resolve<ISomeInterface>(() => new SomeObject());
            var obj2 = ioc.Resolve<ISomeInterface2>(() => new SomeObject2());

            Assert.That(obj, Is.TypeOf<SomeObject>());
            Assert.That(obj2, Is.TypeOf<SomeObject2>());
        }

        [Test]
        public void And_instance_is_previously_registered_It_is_resolved_like_this()
        {
            var ioc = IoC.New();
            var obj = new SomeObject();

            ioc.RegisterInstance<ISomeInterface>(obj);

            var obj2 = ioc.Resolve<ISomeInterface>();
            Assert.That(obj, Is.SameAs(obj));
        }
    }

    [TestFixture]
    class When_fluent_api_is_used
    {
        [Test]
        public void It_should_be_used_like_this()
        {
            var ioc = IoC.New();

            var obj = new SomeObject();

            ioc.Setup()
                .Map<ISomeInterface>().To(obj)
                .Map<ISomeInterface2>().To(() => new SomeObject2());

            var obj2 = ioc.Resolve<ISomeInterface>();
            var obj3 = ioc.Resolve<ISomeInterface2>();

            Assert.That(obj2, Is.SameAs(obj));
            Assert.That(obj3, Is.TypeOf<SomeObject2>());
        }

        [Test]
        public void With_mocks_It_may_look_like_this()
        {
            var mock = new Mock<ISomeInterface>();
            var mock2 = new Mock<ISomeInterface2>();

            var ioc = IoC.New(mock, mock2);

            var obj2 = ioc.Resolve<ISomeInterface>();
            var obj3 = ioc.Resolve<ISomeInterface2>();

            Assert.That(obj2, Is.SameAs(mock.Object));
            Assert.That(obj3, Is.SameAs(mock2.Object));
        }
    }
}