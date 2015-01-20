using System;
using System.IO;
using System.Linq;
using System.Reflection;
using L4p.Common.Extensions;
using L4p.Common.Helpers;
using L4p.Common.Loggers;

namespace L4p.Common.Plugins
{
    public interface IPluginLoader
    {
        T LoadImplementationOf<T>(Type likeThis, Func<T> factory = null) where T : class;
    }

    public class PluginLoader : IPluginLoader
    {
        #region Config

        public enum BaseFolderAt
        {
            NearMainModule,
            NearPluginModule,
            SiteRootFolder
        }
        
        public class Config
        {
            public BaseFolderAt BaseFolderAt { get; set; }
            public TimeSpan AccessNoSoonerThan { get; set; }
            public string PluginFolder { get; set; }
            public string ShadowFolder { get; set; }
            public int ShadowSuffixLength { get; set; }
            public bool BeepOnReload { get; set; }

            public Config()
            {
                BaseFolderAt = BaseFolderAt.SiteRootFolder;
                AccessNoSoonerThan = 2.Seconds();
                PluginFolder = ".plugins";
                ShadowFolder = ".shadows";
                ShadowSuffixLength = 8;
                BeepOnReload = false;
            }
        }

        #endregion

        #region Request

        class Request
        {
            public DateTime Now { get; set; }
            public Type LikeThis { get; set; }
            public Type ImplementationType { get; set; }
            public Type InterfaceType { get; set; }
            public string BaseFolder { get; set; }
            public string AssemblyPath { get; set; }
            public string PluginPath { get; set; }
            public string ShadowPath { get; set; }
        }

        #endregion

        #region members

        private readonly Config _config;
        private readonly ILogFile _log;
        private readonly IPluginRepo _repo;

        #endregion

        #region singleton

        private static readonly IPluginLoader _instance;

        static PluginLoader()
        {
            _instance = New();
        }

        public static IPluginLoader Instance
        {
            get { return _instance; }
        }

        #endregion

        #region construction

        public static IPluginLoader New(Config config = null)
        {
            return
                new PluginLoader(config ?? new Config());
        }

        private PluginLoader(Config config)
        {
            _config = config;
            _log = LogFile.New("plugin-loader.log");
            _repo = SyncedDllRepo.New(PluginRepo.New());
        }

        #endregion

        #region private

        private void validate_t_is_interface(Type type)
        {
            if (type.IsInterface)
                return;

            throw
                new PluginException("'{0}' should be an interface", type.Name);
        }

        private string get_base_folder(string rootFolder, Type likeThis)
        {
            switch (_config.BaseFolderAt)
            {
                case BaseFolderAt.NearMainModule:
                default:
                    return AppDomain.CurrentDomain.BaseDirectory;

                case BaseFolderAt.NearPluginModule:
                    return Path.GetDirectoryName(likeThis.Assembly.Location);

                case BaseFolderAt.SiteRootFolder:
                    Validate.NotEmpty(rootFolder);
                    return rootFolder;
            }
        }

        private InstanceBox get_instance_box(Request request)
        {
            var box = _repo.GetInstanceBox(request.InterfaceType);

            if (box == null)
            {
                box = new InstanceBox
                {
                    InterfaceType = request.InterfaceType,
                    ImplementationType = request.LikeThis
                };

                box = _repo.AddInstanceBox(request.InterfaceType, box);
            }

            return box;
        }

        private bool check_plugin_is_up_to_date(Request request, InstanceBox box, out DateTime modifiedAt)
        {
            modifiedAt = box.LastCheckedAt;

            if (request.Now - box.LastCheckedAt < _config.AccessNoSoonerThan)
                return true;

            box.LastCheckedAt = request.Now;

            if (!File.Exists(request.PluginPath))
                return true;

            var assemblyModifiedAt = File.GetLastWriteTimeUtc(request.AssemblyPath);
            var pluginModifiedAt = File.GetLastWriteTimeUtc(request.PluginPath);

            if (assemblyModifiedAt >= pluginModifiedAt)      // primary version is newer
                return true;

            if (box.LoadedAt >= pluginModifiedAt)
                return true;

            modifiedAt = pluginModifiedAt;

            return false;
        }

        private void shadow_copy(Request request)
        {
            var shadowFolder = Path.GetDirectoryName(request.ShadowPath);

            Try.Catch.Wrap(
                () => Directory.CreateDirectory(shadowFolder),
                ex => ex.WrapWith<PluginException>("Failed to create shadow folder '{0}'", shadowFolder));

            Try.Catch.Wrap(_log,
                () => File.Copy(request.PluginPath, request.ShadowPath),
                ex => ex.WrapWith<PluginException>("Failed to shadow copy dll; path='{0}' shadowPath='{1}'", request.PluginPath, request.ShadowPath));

            _log.Info("Shadow copy is done; path='{0}' shadowPath='{1}'", request.PluginPath, request.ShadowPath);
        }

        private Assembly try_help_dot_net_with_resolving(object sender, ResolveEventArgs args)
        {
            var domain = (AppDomain) sender;
            Assembly firstMatch = null;

            foreach (var assembly in domain.GetAssemblies())
            {
                if (assembly.FullName == args.Name)
                {
                    if (firstMatch == null)
                        firstMatch = assembly;

                    _log.Trace("Resolving assembly: a match is found: '{0}' '{1}'", assembly.FullName, assembly.Location);
                }
            }

            return firstMatch;
        }

        private Assembly resolve_assembly(object sender, ResolveEventArgs args)
        {
            var location = args.RequestingAssembly.Location;

            var assembly = try_help_dot_net_with_resolving(sender, args);

            if (assembly == null)
                _log.Warn("Resolving assembly: no match is found; '{1}' ({2})", args.Name, location);

            return assembly;
        }

        private Assembly assembly_resolve_handler(object sender, ResolveEventArgs args)
        {
            var requesingLocation = args.RequestingAssembly.Location;

            _log.Trace("Resolving assembly: '{0}' (requesting assembly = '{1}')", args.Name, requesingLocation);

            var assembly = resolve_assembly(sender, args);

            if (assembly != null)
                _log.Trace("Resolving assembly: resolved to '{0}' '{1}'", assembly.FullName, assembly.Location);

            return assembly;
        }

        private Type query_instance_type(Assembly assembly, Request request)
        {
            var interfaceType = request.InterfaceType;
            var name = request.ImplementationType.Name;

            Type[] types;

            AppDomain.CurrentDomain.AssemblyResolve += assembly_resolve_handler;

            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                var inners = ex.LoaderExceptions;
                foreach (var inner in inners)
                    _log.Error("LoaderException: {0}", inner.Message);

                throw;
            }

            var query = 
                from type in types
                where 
                    interfaceType.IsAssignableFrom(type) && 
                    interfaceType != type &&
                    type.Name == name
                select type;

            return 
                query.FirstOrDefault();
        }

        private Assembly load_assembly(Request request)
        {
            var startupLocation = request.LikeThis.Assembly.Location;

            // load into Neither context (see http://blogs.msdn.com/b/suzcook/archive/2003/05/29/57143.aspx)

            var assembly = Assembly.LoadFile(request.ShadowPath);

            if (assembly.Location == startupLocation)
                throw new PluginException("Got the same (startup) assembly version ({0})", startupLocation);

            return assembly;
        }

        private void load_implementation_type(Request request, InstanceBox box)
        {
            var path = request.ShadowPath;
            var file = Path.GetFileName(request.PluginPath);
            var type = request.ImplementationType;

            var assembly = Try.Catch.Wrap(_log,
                () => load_assembly(request),
                ex => ex.WrapWith<PluginException>("Failed to load assembly from '{0}'", path));

            _log.Info("Assembly from '{0}' is loaded ({1})", path, assembly.Location);

            var implementationType = Try.Catch.Wrap(_log,
                () => query_instance_type(assembly, request),
                ex => ex.WrapWith<PluginException>("Failed to query for instance type '{0}' in '{1}'", type.Name, assembly.Location));

            if (implementationType == null)
                throw new PluginException("Failed to find type that implements '{0}' in '{1}' ({2})", type.Name, file, path);

            _log.Info("Instance type '{0}' is loaded ({1})", implementationType.Name, implementationType.Assembly.Location);

            if (_config.BeepOnReload)
                Console.Beep();

            request.ImplementationType = implementationType;

            box.ImplementationType = implementationType;
            box.LoadedAt = request.Now;
            box.LoadedFrom = path;
        }

        private void create_instance(InstanceBox box, Func<object> factory)
        {
            object instance;

            do
            {
                if (factory != null)
                {
                    instance = factory();
                    break;
                }

                instance = Try.Catch.Wrap(_log,
                    () => Activator.CreateInstance(box.ImplementationType),
                    ex => ex.WrapWith<PluginException>("Failed to create instance of type '{0}' from '{1}'", box.ImplementationType, box.LoadedFrom));
            }
            while (false);

            box.Instance = instance;
        }

        private InstanceBox load_instance_box(Request request)
        {
            var box = get_instance_box(request);

            DateTime modifiedAt;
            bool isUpToDate = check_plugin_is_up_to_date(request, box, out modifiedAt);

            if (isUpToDate)
                return box;

            box.Instance = null;
            _log.Info("'{0}' has a newer plugin version at '{1}' ({2}), '{3}' will be recreated", 
                request.AssemblyPath, request.PluginPath, modifiedAt, request.LikeThis.Name);

            shadow_copy(request);
            load_implementation_type(request, box);

            return box;
        }

        private object get_instance(InstanceBox box, Func<object> factory)
        {
            if (box.Instance != null)
                return box.Instance;

            create_instance(box, factory);

            var instanceType = box.Instance.GetType();
            _log.Trace("Instance of type '{0}' is created ({1})", instanceType.Name, instanceType.Assembly.Location);

            return
                box.Instance;
        }

        #endregion

        #region interface

        T IPluginLoader.LoadImplementationOf<T>(Type likeThis, Func<T> factory)
        {
            var interfaceType = typeof(T);
            validate_t_is_interface(interfaceType);

            var suffix = Guid.NewGuid().ToString("N").Substring(0, _config.ShadowSuffixLength);

            var rootFolder = "";
            var baseFolder = get_base_folder(rootFolder, likeThis);
            var assemblyPath = likeThis.Assembly.Location;
            var pluginName = Path.GetFileName(assemblyPath);
            var pluginPath = Path.Combine(baseFolder, _config.PluginFolder, pluginName);
            var shadowPath = Path.Combine(baseFolder, _config.ShadowFolder, pluginName) + "." + suffix;

//            _log.Trace("Loading pluggable implementation for '{0}' based on '{1}' ({2})", interfaceType.Name, likeThis.Name, assemblyPath);

            var request = new Request
            {
                Now = DateTime.UtcNow,
                LikeThis = likeThis,
                ImplementationType = likeThis,
                InterfaceType = interfaceType,
                BaseFolder = baseFolder,
                AssemblyPath = assemblyPath,
                PluginPath = pluginPath,
                ShadowPath = shadowPath,
            };

            var box = load_instance_box(request);
            var instance = get_instance(box, factory);

            return
                (T) instance;
        }

        #endregion
    }
}