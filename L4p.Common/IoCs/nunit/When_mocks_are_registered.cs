using Moq;
using NUnit.Framework;
using L4p.Common.NUnits;

namespace L4p.Common.IoCs.nunit
{
    [TestFixture]
    class When_mocks_are_registered
    {
        [Test]
        public void They_should_not_be_mixed_with_not_mocks()
        {
            ISomeInterface obj = new SomeObject();
            ISomeInterface2 obj2 = new SomeObject2();

            var mock = new Mock<ISomeInterface>();
            var mock2 = new Mock<ISomeInterface2>();

            AssertIt.Succeeds(() => IoC.New(obj, obj2));
            AssertIt.Succeeds(() => IoC.New(mock, mock2));

            AssertIt.Fails(() => IoC.New(obj, mock));
        }
    }
}