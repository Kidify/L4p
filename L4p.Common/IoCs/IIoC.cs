using System;
using L4p.Common.Extensions;

namespace L4p.Common.IoCs
{
    /// <summary>
    /// Resolving is done as follows
    ///     1 - resolve through a registered instance
    ///     2 - resolve through a registered factory
    ///     3 - resolve through a local factory if provided
    /// </summary>
    public interface IIoC
    {
        IIoC RegisterInstance<T>(object impl) where T: class;
        IIoC RegisterFactory<T>(Func<object> factory) where T : class;
        IIoC RegisterLazy<T>(Func<T> lazy) where T : class;
        T Resolve<T>() where T : class;
        T Resolve<T>(Func<T> factory) where T : class;
        T Resolve<T>(Func<IIoC, T> factory) where T : class;
        T SingleInstance<T>(Func<T> factory) where T : class;
        T SingleInstance<T>(Func<IIoC, T> factory) where T : class;
        void Resolve<T>(Action<T> whenRegistered) where T : class;
        bool HasImplementationFor<T>() where T : class;
    }
}