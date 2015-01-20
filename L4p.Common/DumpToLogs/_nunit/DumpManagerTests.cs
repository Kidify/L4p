using System.Dynamic;
using Moq;
using NUnit.Framework;
using L4p.Common.Loggers;
using L4p.Common.PubSub.client;
using L4p.Common.Wipers;

namespace L4p.Common.DumpToLogs._nunit
{
    [TestFixture]
    public class DumpManagerTests
    {
        private ExpandoObject dump_func(dynamic root)
        {
            root = new ExpandoObject();
            root.Status = true;

            return root;
        }

        [Test]
        public void use_dump_manager()
        {
            var componentName = "use_dump_manager";

            var signals = SignalsManagerEx.New();
            var dm = DumpManager.New(signals);
            var wiper = Wiper.New();

            var log = new Mock<ILogFile>();

            log.Setup(x => x.Name).Returns(componentName);

            dm.Register(dump_func, log.Object, null, wiper);
            signals.Publish(new DumpToLogMsg {ComponentName = componentName});

            log.Verify(
                x => x.Info(It.IsAny<string>()), Times.AtLeastOnce());

            wiper.Proceed();
        }
    }
}