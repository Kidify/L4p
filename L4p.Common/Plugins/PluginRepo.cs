using System;
using System.Collections.Generic;
using L4p.Common.Helpers;

namespace L4p.Common.Plugins
{
    interface IPluginRepo
    {
        InstanceBox GetInstanceBox(Type type);
        InstanceBox AddInstanceBox(Type type, InstanceBox box);
    }

    class PluginRepo : IPluginRepo
    {
        #region members

        private readonly Dictionary<Type, InstanceBox> _instances;

        #endregion

        #region construction

        public static IPluginRepo New()
        {
            return
                new PluginRepo();
        }

        private PluginRepo()
        {
            _instances = new Dictionary<Type, InstanceBox>();
        }

        #endregion

        #region private
        #endregion

        #region interface

        InstanceBox IPluginRepo.GetInstanceBox(Type type)
        {
            InstanceBox box;
            _instances.TryGetValue(type, out box);
            return box;
        }

        InstanceBox IPluginRepo.AddInstanceBox(Type type, InstanceBox box)
        {
            Validate.NotNull(box);

            InstanceBox prev;

            if (_instances.TryGetValue(type, out prev))
                return prev;

            _instances.Add(type, box);
            return box;
        }

        #endregion
    }

    class SyncedDllRepo : IPluginRepo
    {
        private readonly object _mutex = new object();
        private readonly IPluginRepo _impl;

        public static IPluginRepo New(IPluginRepo impl) { return new SyncedDllRepo(impl); }
        private SyncedDllRepo(IPluginRepo impl) { _impl = impl; }

        InstanceBox IPluginRepo.GetInstanceBox(Type type) { lock (_mutex) return _impl.GetInstanceBox(type); }
        InstanceBox IPluginRepo.AddInstanceBox(Type type, InstanceBox box) { lock (_mutex) return _impl.AddInstanceBox(type, box); }
    }
}