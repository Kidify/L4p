using L4p.Common.Loggers;

namespace L4p.Common.ActiveObjects.nunit
{
    class ActiveTest : ActiveObject
    {
        public ActiveTest()
            : base(LogFile.Console)
        { }
    }
}