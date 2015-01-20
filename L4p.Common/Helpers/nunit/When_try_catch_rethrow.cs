using System;
using NUnit.Framework;
using L4p.Common.Loggers;

namespace L4p.Common.Helpers.nunit
{
    [TestFixture]
    class When_try_catch_rethrow_With_action
    {
        [Test]
        public void It_should_rethrow()
        {
            Assert.Fail();
        }

        [Test]
        public void It_should_perform_action()
        {
            Assert.Fail();
        }

        [Test]
        public void Is_used_as_Wrap_It_should_wrap()
        {
            Try.Catch.Rethrow(
                () => { },
                ex => new Exception("test", ex));

            Assert.Fail();
        }

        [Test]
        public void Is_used_with_telemetry_It_should_rethrow_original_excepton()
        {
            var log = LogFile.Console;

            Try.Catch.Rethrow(
                () => { throw new Exception(); },
                ex => log.Error(ex));

            Assert.Fail();
        }
    }

    [TestFixture]
    class When_try_catch_rethrow_With_func
    {
        [Test]
        public void It_should_rethrow()
        {
            Assert.Fail();
        }

        [Test]
        public void It_should_perform_action()
        {
            Assert.Fail();
        }

        [Test]
        public void Is_used_as_Wrap_It_should_wrap()
        {
            Try.Catch.Rethrow(
                () => { },
                ex => new Exception("test", ex));

            Assert.Fail();
        }
    }

    [TestFixture]
    class When_try_catch_handle
    {
        [Test]
        public void ToDo_Write_test_per_handle_method()
        {
            Assert.Fail();
        }
    }


    [TestFixture]
    class When_try_catch_wrap
    {
        [Test]
        public void ToDo_Write_test_per_wrap_method()
        {
            Assert.Fail();
        }
    }
}