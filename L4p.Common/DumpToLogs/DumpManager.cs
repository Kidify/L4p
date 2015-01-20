using System;
using System.Dynamic;
using System.IO;
using L4p.Common.Extensions;
using L4p.Common.Helpers;
using L4p.Common.Json;
using L4p.Common.Loggers;
using L4p.Common.PubSub;
using L4p.Common.Wipers;

namespace L4p.Common.DumpToLogs
{
    public delegate ExpandoObject DumpFunc(dynamic root);

    public interface IDumpManager
    {
        void Register(DumpFunc dumpFunc, ILogFile log, IAmAComponent component, IWiper wiper = null);
        void DumpComponent(string componentName);
    }

    public class DumpManager : IDumpManager
    {
        #region members

        private readonly ISignalsManager _signals;
        private readonly IDumpRepo _repo;
        private readonly ILogFile _bootstrap;

        #endregion

        #region singleton

        private static IDumpManager _instance;

        static DumpManager()
        {
            _instance = New();
        }

        public static IDumpManager Instance
        {
            get { return _instance; }
        }

        public static void Register<T>(DumpFunc dumpFunc = null, IWiper wiper = null)
            where T : IAmAComponent, new()
        {
            var component = new T();
            var log = LogFile.New(component.LogName);

            _instance.Register(dumpFunc, log, component, wiper);
        }

        #endregion

        #region construction

        public static IDumpManager New(ISignalsManager signals = null)
        {
            return
                new DumpManager(signals ?? Signals.Instance);
        }

        private DumpManager(ISignalsManager signals)
        {
            _signals = signals;
            _repo = DumpRepo.NewSync();
            _bootstrap = LogFile.New("bootstrap.log", true);
        }

        #endregion

        #region private

        private static string make_dump(DumpFunc dumpFunc)
        {
            try
            {
                var dump = dumpFunc(null);
                var json = dump.AsReadableJson();

                return json;
            }
            catch (Exception ex)
            {
                TraceLogger.WriteLine(ex);
                return null;
            }
        }

        private static void got_dump_to_log_msg(IDumpManager self, DumpToLogMsg msg)
        {
            self.DumpComponent(msg.ComponentName);
        }

        #endregion

        #region interface

        void IDumpManager.Register(DumpFunc dumpFunc, ILogFile log, IAmAComponent component, IWiper wiper)
        {
            Validate.NotNull(log);

            if (component == null)
                component = new UnknownComponent();

            wiper = wiper.WrapIfNull();

            string componentName = component.Name;
            Validate.NotEmpty(componentName);

            string lowerName = componentName.ToLowerInvariant();

            var info = new DumpInfo {
                Component = component,
                Log = log,
                DumpFunc = dumpFunc
            };

            _repo.AddComponent(info);

            wiper.que += 
                () => _repo.RemoveComponent(componentName);

            var slot = _signals.SubscribeTo<DumpToLogMsg>(
                msg => got_dump_to_log_msg(this, msg),
                msg => msg.ComponentName == lowerName);

            wiper.que += slot.Cancel;

            var wellcome = "Component '{0}' version '{1}' is here".Fmt(component.Name, component.Version);

            _bootstrap.Info(wellcome);
            log.Info(wellcome);
        }

        void IDumpManager.DumpComponent(string componentName)
        {
            var info = _repo.GetComponent(componentName);

            if (info == null)
                return;

            var dump = make_dump(info.DumpFunc);

            if (dump.IsEmpty())
                return;

            info.Log.Info(dump);
        }

        #endregion
    }
}