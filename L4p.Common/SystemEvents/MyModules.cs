using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using L4p.Common.Extensions;
using L4p.Common.Helpers;
using L4p.Common.Json;
using L4p.Common.Loggers;

namespace L4p.Common.SystemEvents
{
    interface IMyModules
    {
        Type[] GetInitializersTypes(Assembly[] entryAssemblies);
        IShouldRunOnSystemStart[] InstantiateInitializers(Type[] types);
        IShouldRunOnSystemStart[] OrderInitializers(IShouldRunOnSystemStart[] instances);
        int CallInitializers(string moduleKey, IShouldRunOnSystemStart[] instances);
    }

    class MyModules : IMyModules
    {
        #region config

        class Config
        {
            public int MaxRecursionDepthOnLoad = 5;
        }

        #endregion

        #region members

        private readonly Config _config;
        private readonly ILogFile _log;

        #endregion

        #region construction

        public static IMyModules New(ILogFile log)
        {
            return
                new MyModules(log);
        }

        private MyModules(ILogFile log)
        {
            _config = new Config();
            _log = log;
        }

        #endregion

        #region private

        private bool is_system_assembly(Assembly assembly)
        {
            var name = assembly.GetName().Name;

            if (name.StartsWith("mscorlib"))
                return true;

            if (name.StartsWith("Microsoft."))
                return true;

            if (name.StartsWith("System."))
                return true;

            return false;
        }

        private IShouldRunOnSystemStart create_instance(Type type)
        {
            try
            {
                return
                    (IShouldRunOnSystemStart) Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "MyModules: failed to instantiate type '{0}'", type.Name);
                return null;
            }
        }

        private bool call_initializer(string moduleKey, IShouldRunOnSystemStart instance)
        {
            try
            {
                instance.SystemIsBeingStarted(moduleKey, _log);
                return true;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "MyModules: failed to initialize instance of type '{0}'", instance.GetType().Name);
                return false;
            }
        }

        private Assembly load_assembly(AssemblyName assemblyName)
        {
            try
            {
                return
                    Assembly.Load(assemblyName);
            }
            catch (Exception ex)
            {
                _log.Warn("MyModules: failed to load assembly '{0}'; {1}", assemblyName.Name, ex.Message);
                return null;
            }
        }

        private void load_references_of(Assembly assembly, HashSet<string> loadedSet, int depth)
        {
            if (is_system_assembly(assembly))
                return;

            if (depth > _config.MaxRecursionDepthOnLoad)
            {
                _log.Warn("MyModules: skipping '{0}' since depth={1}", assembly.GetName(), depth);
                return;
            }

            foreach (var referenceName in assembly.GetReferencedAssemblies())
            {
                if (loadedSet.Contains(referenceName.Name))
                    continue;

                loadedSet.Add(referenceName.Name);
                var reference = load_assembly(referenceName);

                if (reference == null)
                    continue;

                load_references_of(reference, loadedSet, depth + 1);
            }
        }

        private void load_all_referenced_assemblies(Assembly[] entryAssemblies)
        {
            var loadedSet = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var assembly in entryAssemblies)
            {
                if (assembly == null)
                    continue;

                load_references_of(assembly, loadedSet, 0);
            }

            var sortedNames =
                from name in loadedSet
                orderby name
                select name;

            _log.Trace("MyModules: loaded modules are {0}", sortedNames.ToArray().ToJson());
        }

        private Type[] load_types(Assembly assembly)
        {
            try
            {
                return
                    assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                var sb = new StringBuilder();

                sb
                    .AppendFormat("MyModules: failed to load types of '{0}' assembly ({1} errors); returning {2} loaded types", 
                        assembly.GetName().Name, ex.LoaderExceptions.Length, ex.Types.Length);

                foreach (var cause in ex.LoaderExceptions)
                {
                    sb
                        .StartNewLine()
                        .AppendFormat("  --> {0}", cause.Message);
                }

                _log.Warn(sb.ToString());

                return 
                    ex.Types;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "MyModules: failed to load types of '{0}' assembly", assembly.GetName().Name);
                return null;
            }
        }

        #endregion

        #region interface

        Type[] IMyModules.GetInitializersTypes(Assembly[] entryAssemblies)
        {
            try
            {
                load_all_referenced_assemblies(entryAssemblies);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "MyModules: failed to load referenced assemblies");
            }

            var myInterface = typeof(IShouldRunOnSystemStart);
            var list = new List<Type>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (is_system_assembly(assembly))
                    continue;

                var types = load_types(assembly);

                if (types.IsEmpty())
                    continue;

                foreach (var type in types)
                {
                    if (type == null)
                        continue;

                    if (type.IsInterface)
                        continue;

                    if (!myInterface.IsAssignableFrom(type))
                        continue;

                    list.Add(type);
                }
            }

            return
                list.ToArray();
        }

        IShouldRunOnSystemStart[] IMyModules.InstantiateInitializers(Type[] types)
        {
            var list = new List<IShouldRunOnSystemStart>();

            foreach (var type in types)
            {
                var instance = create_instance(type);

                if (instance == null)
                    continue;

                list.Add(instance);
            }

            return
                list.ToArray();
        }

        IShouldRunOnSystemStart[] IMyModules.OrderInitializers(IShouldRunOnSystemStart[] instances)
        {
            var query =
                from instance in instances
                orderby instance.InitializationOrder
                select instance;

            return
                query.ToArray();
        }

        int IMyModules.CallInitializers(string moduleKey, IShouldRunOnSystemStart[] instances)
        {
            var count = 0;

            foreach (var instance in instances)
            {
                var ok = call_initializer(moduleKey, instance);

                if (ok == false)
                    continue;

                count++;
            }

            return count;
        }

        #endregion
    }
}