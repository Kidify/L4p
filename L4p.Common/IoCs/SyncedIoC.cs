using System;

namespace L4p.Common.IoCs
{
    public class SyncedIoC : IIoC
    {
        private readonly object _mutex = new object();
        private readonly IIoC _impl;

        public static IIoC New(IIoC impl) { return new SyncedIoC(impl); }
        private SyncedIoC(IIoC impl) { _impl = impl; }

        IIoC IIoC.RegisterInstance<T>(object impl) { lock(_mutex) return _impl.RegisterInstance<T>(impl); }
        IIoC IIoC.RegisterFactory<T>(Func<object> factory) { lock(_mutex) return _impl.RegisterFactory<T>(factory); }
        IIoC IIoC.RegisterLazy<T>(Func<T> lazy) { lock(_mutex) return _impl.RegisterLazy(lazy); }
        T IIoC.Resolve<T>() { lock (_mutex) return _impl.Resolve<T>(); }
        T IIoC.Resolve<T>(Func<T> factory) { lock(_mutex) return _impl.Resolve(factory); }
        T IIoC.Resolve<T>(Func<IIoC, T> factory) { lock (_mutex) return _impl.Resolve(factory); }
        T IIoC.SingleInstance<T>(Func<T> factory) { lock(_mutex) return _impl.SingleInstance(factory); }
        T IIoC.SingleInstance<T>(Func<IIoC, T> factory) { lock (_mutex) return _impl.SingleInstance(factory); }
        void IIoC.Resolve<T>(Action<T> whenRegistered) { lock (_mutex) _impl.Resolve(whenRegistered); }
        bool IIoC.HasImplementationFor<T>() { lock(_mutex) return _impl.HasImplementationFor<T>(); }
    }
}