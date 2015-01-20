using Moq;
using NUnit.Framework;

namespace L4p.Common.Concerns.nunit
{
    [TestFixture]
    class When_splitting_to_one
    {
        [Test]
        public void It_should_invoke_both_implementations()
        {
            var obj = new Mock<ISomeInterface>();
            var decorator = new Mock<ISomeInterface>();

            var splitter = ToOneSplitter.New(obj.Object, decorator.Object);
            splitter.SomeMethod(42);

            obj.Verify(x => x.SomeMethod(42));
            decorator.Verify(x => x.SomeMethod(42));
        }

        [Test]
        public void It_should_return_result_from_implementation()
        {
            var obj = new Mock<ISomeInterface>();
            var decorator = new Mock<ISomeInterface>();

            obj.Setup(x => x.SomeFunction("test"))
                .Returns(42);

            var splitter = ToOneSplitter.New(obj.Object, decorator.Object);
            int result = splitter.SomeFunction("test");

            Assert.That(result, Is.EqualTo(42));
        }
    }
}