using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using L4p.Common.DumpToLogs;
using L4p.Common.PubSub.client;
using L4p.Common.PubSub.contexts;

namespace L4p.Common.PubSub
{
    public interface ISignalsManager : IHaveDump
    {
        ISignalSlot SubscribeTo<T>(Action<T> callback, Func<T, bool> filter = null, StackFrame filterAt = null) 
            where T : class, new();

        void Publish<T>(T msg) where T : class, new();
        ISessionContext Context { get; }

        void StartAgent();
        void StopAgent();
    }

    public static class Signals
    {
        #region singleton

        private static ISignalsManager _signals = NotInitializedSignals.New();

        public static ISignalsManager Instance
        {
            get { return _signals; }
            set { _signals = value; }
        }

        #endregion

        #region interface

        public static ISessionContext Context 
        {
            get { return _signals.Context; }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ISignalSlot SubscribeTo<T>(Action<T> handler, Func<T, bool> filter = null)
            where T : class, new()
        {
            var filterAt = new StackFrame(1);

            return
                _signals.SubscribeTo(handler, filter, filterAt);
        }

        public static void Publish<T>(T msg)
            where T : class, new()
        {
            _signals.Publish(msg);
        }

        #endregion
    }
}