using NUnit.Framework;
using L4p.Common.Loggers;

namespace L4p.Common.Wipers._nunit
{
    [TestFixture]
    class When_wiper_is_used
    {
        [Test]
        public void It_should_clean_up()
        {
            var wiper = Wiper.New(LogFile.Null);

            var ok = false;

            wiper.que += () => ok = true;
            wiper.Proceed();

            Assert.True(ok);
        }

    }
}