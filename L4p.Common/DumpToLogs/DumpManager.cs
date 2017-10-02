using System;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading;
using L4p.Common.Extensions;
using L4p.Common.Helpers;
using L4p.Common.Json;
using L4p.Common.Loggers;
using L4p.Common.PubSub;
using L4p.Common.PubSub.client;
using L4p.Common.Schedulers;
using L4p.Common.Wipers;

namespace L4p.Common.DumpToLogs
{
    public delegate ExpandoObject DumpFunc(dynamic root);

    public interface IDumpManager : IHaveDump
    {
        void Register(DumpFunc dumpFunc, ILogFile log, IAmAComponent component, IWiper wiper = null);
        void DumpComponent(DumpToLogMsg msg);
    }

    public class DumpManager : IDumpManager
    {
        #region counters

        class Counters
        {
            public int UnknownComponents;
            public int Registrations;
            public int DumpRequests;
            public int DumpsDone;
            public int DumpFunctionsInvoked;
            public int DumpFunctionFailed;
        }

        #endregion

        #region members

        private readonly Counters _counters;
        private readonly IDumpRepo _repo;
        private readonly ILogFile _bootstrap;

        #endregion

        #region singleton

        private static readonly IDumpManager _instance;

        static DumpManager()
        {
            _instance = New();
            DeferredCall.Start(200.Milliseconds(), () => make_subscriptions(_instance));
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

        public static IDumpManager New()
        {
            return
                new DumpManager();
        }

        private DumpManager()
        {
            _counters = new Counters();
            _repo = DumpRepo.NewSync();
            _bootstrap = LogFile.New("bootstrap.log", true);
        }

        #endregion

        #region private

        private static void make_subscriptions(IDumpManager self)
        {
            Register<DumpComponent>(self.Dump);

            var signals = Signals.Instance;
            var globalSignalsAreInitialized = !(signals is NotInitializedSignals);

            if (globalSignalsAreInitialized)
                signals.SubscribeTo<DumpToLogMsg>(self.DumpComponent);
        }

        private string get_class_name(DumpFunc dumpFunc)
        {
            if (dumpFunc == null)
                return "n/a";

            var target = dumpFunc.Target;

            if (target == null)
                return "n/a";

            return
                target.GetType().Name;
        }

        private ExpandoObject call_single_dump(DumpFunc dumpFunc, ILogFile log)
        {
            if (dumpFunc == null)
                return null;

            dynamic dump = new ExpandoObject();

            var className = get_class_name(dumpFunc);
            dump.__Class = className;

            try
            {
                dump = dumpFunc(dump);
                Interlocked.Increment(ref _counters.DumpFunctionsInvoked);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                Interlocked.Increment(ref _counters.DumpFunctionFailed);
            }

            return dump;
        }

        private string make_dump(DumpInfo info)
        {
            dynamic root = new ExpandoObject();

            var component = info.Component;
            root.Component = "Component '{0}' '{1}'".Fmt(component.Name, component.Version);

            var calls =
                from dumpFunc in info.Dumps
                let dump = call_single_dump(dumpFunc, info.Log)
                where dump != null
                select dump;

            root.Dumps = calls.ToArray();
            var json = JsonHelpers.AsReadableJson(root);

            return json;
        }

        #endregion

        #region interface

        ExpandoObject IHaveDump.Dump(dynamic root)
        {
            if (root == null)
                root = new ExpandoObject();

            root.Counters = _counters;
            root.Repo = _repo.Dump();

            return root;
        }

        void IDumpManager.Register(DumpFunc dumpFunc, ILogFile log, IAmAComponent component, IWiper wiper)
        {
            Validate.NotNull(log);

            if (component == null)
            {
                component = new UnknownComponent();
                Interlocked.Increment(ref _counters.UnknownComponents);
            }

            wiper = wiper.WrapIfNull();

            string componentName = component.Name;
            Validate.NotEmpty(componentName);

            var info = new DumpInfo {
                Component = component,
                Log = log,
                Dumps = new [] {dumpFunc}
            };

            _repo.AddComponent(info);

            wiper.que += 
                () => _repo.RemoveComponent(componentName);

            var className = get_class_name(dumpFunc);
            var wellcome = "Component '{0}' '{1}' ({2}) is registered for dumps".Fmt(component.Name, component.Version, className);

            _bootstrap.Info(wellcome);
            log.Info(wellcome);

            Interlocked.Increment(ref _counters.Registrations);
        }

        void IDumpManager.DumpComponent(DumpToLogMsg msg)
        {
            Interlocked.Increment(ref _counters.DumpRequests);

            var componentName = msg.ComponentName;

            if (componentName.IsEmpty())
                return;

            var info = _repo.GetComponent(componentName);

            if (info == null)
                return;

            var dump = make_dump(info);

            if (dump.IsEmpty())
                return;

            var dumps = info.Dumps ?? new DumpFunc[0];

            info.Log.Info("Dump of '{0}' ({1} dumps) \r\n{2}", componentName, dumps.Length, dump);

            Interlocked.Increment(ref _counters.DumpsDone);
        }

        #endregion
    }
}