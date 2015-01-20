using System;
using System.Collections.Generic;
using L4p.Common.Helpers;
using L4p.Common.Loggers;

namespace L4p.Common.DumpToLogs
{
    class DumpInfo
    {
        public IAmAComponent Component { get; set; }
        public ILogFile Log { get; set; }
        public DumpFunc DumpFunc { get; set; }
    }

    interface IDumpRepo
    {
        void AddComponent(DumpInfo info);
        DumpInfo GetComponent(string componentName);
        void RemoveComponent(string componentName);
    }

    class DumpRepo : IDumpRepo
    {
        #region members

        private readonly Dictionary<string, DumpInfo> _infos;

        #endregion

        #region construction

        public static IDumpRepo New()
        {
            return
                new DumpRepo();
        }

        public static IDumpRepo NewSync()
        {
            return
                SyncDumpRepo.New(
                new DumpRepo());
        }

        private DumpRepo()
        {
            _infos = new Dictionary<string, DumpInfo>(StringComparer.InvariantCultureIgnoreCase);
        }

        #endregion

        #region private
        #endregion

        #region interface

        void IDumpRepo.AddComponent(DumpInfo info)
        {
            Validate.NotNull(info);

            var name = info.Component.Name;
            _infos[name] = info;
        }

        DumpInfo IDumpRepo.GetComponent(string componentName)
        {
            DumpInfo info;
            _infos.TryGetValue(componentName, out info);

            return info;
        }

        void IDumpRepo.RemoveComponent(string componentName)
        {
            _infos.Remove(componentName);
        }

        #endregion
    }

    class SyncDumpRepo : IDumpRepo
    {
        private readonly object _mutex = new object();
        private readonly IDumpRepo _impl;

        public static IDumpRepo New(IDumpRepo impl) { return new SyncDumpRepo(impl); }
        private SyncDumpRepo(IDumpRepo impl) { _impl = impl; }

        void IDumpRepo.AddComponent(DumpInfo info) { lock(_mutex) _impl.AddComponent(info); }
        DumpInfo IDumpRepo.GetComponent(string componentName) { lock (_mutex) return _impl.GetComponent(componentName); }
        void IDumpRepo.RemoveComponent(string componentName) { lock (_mutex) _impl.RemoveComponent(componentName); }
    }
}