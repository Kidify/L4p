using NUnit.Framework;

namespace L4p.Common.NUnits
{
    enum TestEnum
    {
        Value,
        Value2 = 2,
    }

    [TestFixture]
    class When_converting_to_enum
    {
        [Test]
        public void The_below_should_succeed()
        {
            Assert.That(Enum<TestEnum>.From(0), Is.EqualTo(TestEnum.Value));
            Assert.That(Enum<TestEnum>.From(2), Is.EqualTo(TestEnum.Value2));

            Assert.That(Enum<TestEnum>.From("Value"), Is.EqualTo(TestEnum.Value));
            Assert.That(Enum<TestEnum>.From("Value2"), Is.EqualTo(TestEnum.Value2));

            Assert.That(Enum<TestEnum>.From("0"), Is.EqualTo(TestEnum.Value));
            Assert.That(Enum<TestEnum>.From("2"), Is.EqualTo(TestEnum.Value2));
        }

        [Test]
        public void The_below_should_fail()
        {
            AssertIt.Fails(() => Enum<TestEnum>.From(1));
            AssertIt.Fails(() => Enum<TestEnum>.From("abc"));
            AssertIt.Fails(() => Enum<TestEnum>.From("1"));
        }
    }
}