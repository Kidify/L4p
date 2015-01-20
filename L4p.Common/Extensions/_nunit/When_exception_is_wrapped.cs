using NUnit.Framework;

namespace L4p.Common.Extensions._nunit
{
    [TestFixture]
    class When_exception_is_wrapped
    {
        public void It_should_be_of_specified_type()
        {
            Assert.Fail();
        }
    }

    [TestFixture]
    class When_exception_is_wrapped_With_data
    {
        public void It_should_be_of_specified_type()
        {
            Assert.Fail();
        }

        public void It_should_contains_data()
        {
            Assert.Fail();
        }
    }
}