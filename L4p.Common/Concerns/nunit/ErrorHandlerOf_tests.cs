using NUnit.Framework;

namespace L4p.Common.Concerns.nunit
{
    [TestFixture]
    class When_try_catch_rethrow_With_method_call
    {
        [Test]
        public void It_should_invoke_underlaying_method()
        {
            Assert.Fail();
        }

        [Test]
        public void And_call_is_failed_It_should_rethrow_error()
        {
            Assert.Fail();
        }
    }

    [TestFixture]
    class When_try_catch_rethrow_With_function_call
    {
        [Test]
        public void It_should_invoke_underlaying_function()
        {
            Assert.Fail();
        }

        [Test]
        public void And_call_is_failed_It_should_rethrow_error()
        {
            Assert.Fail();
        }
    }

    [TestFixture]
    class When_try_catch_handle_With_method_call
    {
        [Test]
        public void It_should_invoke_underlaying_function()
        {
            Assert.Fail();
        }

        [Test]
        public void And_call_is_failed_It_should_handle_error()
        {
            Assert.Fail();
        }
    }

    [TestFixture]
    class When_exception_is_wrapped
    {
        [Test]
        public void It_should_be_of_specified_type()
        {
            Assert.Fail();
        }
    }
}