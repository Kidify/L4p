using System.Dynamic;
using L4p.Common.PubSub;
using Moq;
using NUnit.Framework;
using L4p.Common.Loggers;
using L4p.Common.PubSub.client;
using L4p.Common.Wipers;

namespace L4p.Common.DumpToLogs._nunit
{
    class DummyComponent : IAmAComponent
    {
        public static readonly string Name = "dummy";
        public static readonly string Version = "1.2.3";
        public static readonly string LogName = "dummy.log";

        string IAmAComponent.Name { get { return Name; } }
        string IAmAComponent.Version { get { return Version; } }
        string IAmAComponent.LogName { get { return LogName; } }
    }

    [TestFixture, Explicit]
    public class DumpManagerTests
    {
        private ExpandoObject dump_func(dynamic root)
        {
            root.Status = true;
            return root;
        }

        private ExpandoObject dump_func2(dynamic root)
        {
            root.ALabel = "second dump";
            return root;
        }

        [Test]
        public void one_dump_per_component()
        {
            var dm = DumpManager.New();
            var log = LogFile.Console;

            dm.Register(dump_func, log, new DummyComponent());
            dm.DumpComponent(new DumpToLogMsg {ComponentName = DummyComponent.Name});

            Assert.Pass();
        }


        [Test]
        public void two_dumps_per_component()
        {
            var dm = DumpManager.New();
            var log = LogFile.Console;

            dm.Register(dump_func, log, new DummyComponent());
            dm.Register(dump_func2, log, new DummyComponent());

            dm.DumpComponent(new DumpToLogMsg { ComponentName = DummyComponent.Name });

            Assert.Pass();
        }
    }
}