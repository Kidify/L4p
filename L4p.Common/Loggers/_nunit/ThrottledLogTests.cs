using Moq;
using NUnit.Framework;
using L4p.Common.Extensions;

namespace L4p.Common.Loggers._nunit
{
    [TestFixture]
    class ThrottledLogTests
    {
        [Test]
        public void ThrottledMsgTest()
        {
            var log = new Mock<ILogFile>();
            var tlog = ThrottledLog.New(10.Seconds(), log.Object);

            var msg = "Some error message {0}";

            tlog.Error(msg, 10);
            tlog.Error(msg, 10);

            log.Verify(x => x.Error(msg, 10), Times.Exactly(1));
        }
    }
}