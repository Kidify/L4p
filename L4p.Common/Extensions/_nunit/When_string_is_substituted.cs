using NUnit.Framework;
using L4p.Common.NUnits;

namespace L4p.Common.Extensions._nunit
{
    [TestFixture]
    class When_string_is_substituted
    {
        [Test]
        public void It_should_work()
        {
            Assert.That(
                "{Value} - {Str}".Substitute(new {Value = 123, Str="some string"}),
                Is.EqualTo("123 - some string"));

            Assert.That(
                "{Value} - {Value} - {Str}".Substitute(new { Value = 123, Str = "some string" }),
                Is.EqualTo("123 - 123 - some string"));

            AssertIt.Fails(
                () => "{Value} - {Str} and {other}".Substitute(new { Value = 123, Str = "some string" }));
        }
    }
}