using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using L4p.Common.Extensions;
using L4p.Common.Json;
using L4p.Common.Loggers;

namespace L4p.Common.SystemEvents
{
    public interface IMySystemEvents
    {
        void SystemIsBeingStarted(string moduleKey, params Assembly[] entryAssemblies);
        void SystemIsBeingStopped();
        void SetConsoleCtrlHandler();

        event Action OnSystemStart;
        event Action OnSystemExit;
    }

    public class MySystemEvents : IMySystemEvents
    {
        #region members

        private readonly ILogFile _log;

        private int _systemExitIsRaised;

        private event Action _onSystemStart;
        private event Action _onSystemExit;

        #endregion

        #region singleton

        private static IMySystemEvents _instance;

        public static IMySystemEvents Instance
        {
            get { return _instance; }
        }

        static MySystemEvents()
        {
            _instance = New();
        }

        #endregion

        #region construction

        static IMySystemEvents New()
        {
            return
                new MySystemEvents();
        }

        private MySystemEvents()
        {
            _log = LogFile.New("bootstrap.log");
            _systemExitIsRaised = 0;

            AppDomain.CurrentDomain.ProcessExit += 
                (sender, args) => raise_system_exit_once();
        }

        #endregion

        #region private

        private delegate bool ConsoleEventDelegate(int eventType);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);

        private bool handle_win32_console_event(int eventType)
        {
            Console.Beep();
            console_is_being_manually_closed();

            return false;
        }

        private void console_is_being_manually_closed()
        {
            _log.Info("SystemEvents: Console application is being closed...");
            raise_system_exit_once();
        }

        private void raise_event(Action action, string name)
        {
            if (action == null)
                return;

            try
            {
                action();
            }
            catch (Exception ex)
            {
                _log.Error(ex, "SystemEvents: failure during event '{0}'", name);
            }
        }

        private void raise_system_exit_once()
        {
            if (Interlocked.Increment(ref _systemExitIsRaised) > 1)
                return;

            _log.Info("SystemEvents: system is being stopped");
            raise_event(_onSystemExit, "on exit");
        }

        private void initialize_modules(string moduleKey, Assembly[] entryAssemblies)
        {
            var modules = MyModules.New(_log);

            var types = modules.GetInitializersTypes(entryAssemblies);
            _log.Trace("Initializers types: {0}", types.ToJson());

            var initialzers = 
                modules.OrderInitializers(
                modules.InstantiateInitializers(types));

            var forLog =
                from initializer in initialzers
                select new {initializer.GetType().FullName, initializer.InitializationOrder};

            _log.Trace("Initializers order: {0}", forLog.ToArray().ToJson());

            var count = modules.CallInitializers(moduleKey, initialzers);

            _log.Trace("Initializers: types: {0} instances: {1} initialized: {2}", 
                types.Length, initialzers.Length, count);
        }

        #endregion

        #region interface

        void IMySystemEvents.SystemIsBeingStarted(string moduleKey, Assembly[] entryAssemblies)
        {
            if (entryAssemblies.IsEmpty())
                entryAssemblies = new[] { Assembly.GetEntryAssembly() };

            var entryAssembly = entryAssemblies[0];

            var entryName = 
                entryAssembly != null ? entryAssembly.GetName().Name : "unknown";

            _log.Info("SystemEvents: system is starting (entry='{0}')", entryName);

            try
            {
                initialize_modules(moduleKey, entryAssemblies);
                _log.Info("SystemEvents: modules are initialized");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "SystemEvents: failed to initialize modules");
            }

            raise_event(_onSystemStart, "on start");
        }

        void IMySystemEvents.SystemIsBeingStopped()
        {
            raise_system_exit_once();
        }

        void IMySystemEvents.SetConsoleCtrlHandler()
        {
            // seems to be not working but causing failures
            return;

            #pragma warning disable 162
            var ok = SetConsoleCtrlHandler(handle_win32_console_event, true);

            if (!ok)
            {
                _log.Warn("SystemEvents: failed to set ctrl-handler");
                return;
            }

            _log.Info("SystemEvents: Console ctrl-handler is installed");
            #pragma warning restore 162
        }

        event Action IMySystemEvents.OnSystemStart
        {
            add { _onSystemStart += value; }
            remove { _onSystemStart -= value; }
        }

        event Action IMySystemEvents.OnSystemExit
        {
            add { _onSystemExit += value; }
            remove { _onSystemExit -= value; }
        }

        #endregion
    }
}