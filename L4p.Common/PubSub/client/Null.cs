using System;

namespace L4p.Common.PubSub.client
{
    static class Null
    {
        public static readonly ISignalSlot SignalSlot = new NullSlot();
    }

    class NullSlot : ISignalSlot
    {
        void IDisposable.Dispose()      {}
        void ISignalSlot.Cancel()       {}
    }
}