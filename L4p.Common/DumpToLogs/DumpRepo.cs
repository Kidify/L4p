using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using L4p.Common.Extensions;
using L4p.Common.Helpers;
using L4p.Common.Loggers;

namespace L4p.Common.DumpToLogs
{
    class DumpInfo
    {
        public IAmAComponent Component { get; set; }
        public ILogFile Log { get; set; }
        public DumpFunc[] Dumps { get; set; }
    }

    interface IDumpRepo : IHaveDump
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

        private void aggregate_info(DumpInfo src, DumpInfo dst)
        {
            var dumps = new DumpFunc [dst.Dumps.Length + src.Dumps.Length];

            dst.Dumps.CopyTo(dumps, 0);
            src.Dumps.CopyTo(dumps, dst.Dumps.Length);

            dst.Dumps = dumps;
        }

        #endregion

        #region interface

        ExpandoObject IHaveDump.Dump(dynamic root)
        {
            if (root == null)
                root = new ExpandoObject();

            root.Count = _infos.Count;

            var transform =
                from info in _infos.Values
                let component = info.Component
                orderby component.Name
                select "'{0}' '{1}' log='{2}' dumps={3}".Fmt(
                            component.Name, component.Version, info.Log.Name, info.Dumps.Length);

            root.Components = transform.ToArray();

            return root;
        }

        void IDumpRepo.AddComponent(DumpInfo info)
        {
            Validate.NotNull(info);

            var name = info.Component.Name;

            DumpInfo prev;

            if (_infos.TryGetValue(name, out prev))
            {
                aggregate_info(info, prev);
                return;
            }

            if (info.Log == null)
                info.Log = LogFile.New(info.Component.LogName);

            _infos.Add(name, info);
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

        ExpandoObject IHaveDump.Dump(dynamic root) { lock (_mutex) return _impl.Dump(); }
        void IDumpRepo.AddComponent(DumpInfo info) { lock(_mutex) _impl.AddComponent(info); }
        DumpInfo IDumpRepo.GetComponent(string componentName) { lock (_mutex) return _impl.GetComponent(componentName); }
        void IDumpRepo.RemoveComponent(string componentName) { lock (_mutex) _impl.RemoveComponent(componentName); }
    }
}