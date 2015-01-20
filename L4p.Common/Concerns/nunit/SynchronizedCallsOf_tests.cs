using NUnit.Framework;

namespace L4p.Common.Concerns.nunit
{
    [TestFixture]
    class When_method_call_is_joined
    {
        [Test]
        public void And_same_thread_It_should_be_invoked_on_own_thread()
        {
            Assert.Fail();
        }

        [Test]
        public void And_other_thread_It_should_be_invoked_on_own_thread()
        {
            Assert.Fail();
        }

        [Test]
        public void And_timeout_It_should_throw()
        {
            Assert.Fail();
        }

        [Test]
        public void And_activeMsgTimeout_is_set_It_should_wait_the_maximum_timeout()
        {
            Assert.Fail();
        }
    }

    [TestFixture]
    class When_active_thread_is_validated
    {
        [Test]
        public void Calls_from_other_threads_should_fail()
        {
            Assert.Fail();
        }

        [Test]
        public void SetActiveThread_should_be_called()
        {
            Assert.Fail();
        }
    }

    [TestFixture]
    class When_function_call_is_joined
    {
        [Test]
        public void It_should_return_invocation_result()
        {
            Assert.Fail();
        }

        [Test]
        public void And_same_thread_It_should_be_invoked_on_own_thread()
        {
            Assert.Fail();
        }

        [Test]
        public void And_other_thread_It_should_be_invoked_on_own_thread()
        {
            Assert.Fail();
        }

        [Test]
        public void And_timeout_It_should_throw()
        {
            Assert.Fail();
        }
    }
}