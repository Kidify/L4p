using System;
using System.Threading;
using L4p.Common.Extensions;
using L4p.Common.FunnelsModel.client;
using L4p.Common.FunnelsModel.config;
using L4p.Common.FunnelsModel.io;
using L4p.Common.GcCaches;
using L4p.Common.IoCs;
using L4p.Common.Loggers;
using L4p.Common.Schedulers;


namespace L4p.Common.FunnelsModel
{
    public partial interface IFunnelsManager : IShouldBeStarted
    {
        IFunnelsManager Start();
        void Stop();

        IFunnel GetFunnelOfInstance<T>(T instance) where T : class;
        IFunnel GetFunnelOfClass(Type type);

        IFunnel GetCurrentFunnel();
        IFunnel SetCurrentFunnel(IFunnel funnel);
    }

    interface IFunnelsManagerEx : IFunnelsManager
    {
        void CleanDeadFunnels();
        void PruneFunnelData(Guid storeId, string path);
        void ShopIsRemoved(comm.ShopInfo shopInfo);
        bool ProcessIo();
    }

    public partial class FunnelsManager : IFunnelsManagerEx
    {
        #region members

        private readonly IFmConfigRa _config;
        private readonly ThreadLocal<IFunnel> _current;
        private readonly IIoC _ioc;
        private readonly IFunnelsRepo _repo;
        private readonly IGcCache<IFunnel, IFunnelStore> _cache;
        private readonly IEventScheduler _idler;
        private readonly IFunnelsAgent _agent;
        private readonly IIoSink _sink;
        private readonly IIoThreads _threads;
        private readonly IFmFactoryEngine _factory;

        #endregion

        #region construction

        public static IFunnelsManager New(FunnelsConfig config = null)
        {
            config = config ?? new FunnelsConfig();

            return
                new FunnelsManager(config);
        }

        private static IIoC create_dependencies(FunnelsConfig config, IFunnelsManagerEx self)
        {
            var ioc = IoC.New(self);
            var log = LogFile.New("funnels.log");

            ioc.SingleInstance(() => log);
            ioc.SingleInstance(() => FmConfigRa.New(config));
            ioc.SingleInstance(FunnelsRepo.New);
            ioc.SingleInstance(IoQueue.New);
            ioc.SingleInstance(IoSink.New);
            ioc.SingleInstance(GcCache<IFunnel, IFunnelStore>.New);
            ioc.SingleInstance(() => EventScheduler.New(log));
            ioc.SingleInstance(FunnelsAgent.New);
            ioc.SingleInstance(IoThreads.New);
            ioc.SingleInstance(FmFactoryEngine.New);
            ioc.SingleInstance(IoConnector.New);

            return ioc;
        }

        private FunnelsManager(FunnelsConfig config)
        {
            _current = new ThreadLocal<IFunnel>(() => Null.Funnel);

            var ioc = create_dependencies(config, this);

            _ioc = ioc;
            _config = _ioc.Resolve<IFmConfigRa>();
            _repo = ioc.Resolve<IFunnelsRepo>();
            _cache = ioc.Resolve<IGcCache<IFunnel, IFunnelStore>>();
            _idler = ioc.Resolve<IEventScheduler>();
            _agent = ioc.Resolve<IFunnelsAgent>();
            _sink = ioc.Resolve<IIoSink>();
            _threads = ioc.Resolve<IIoThreads>();
            _factory = ioc.Resolve<IFmFactoryEngine>();
        }

        #endregion

        #region private

        private string type_to_name(Type type)
        {
            var name = type.Name;
            return name;
        }

        private string enum_to_name<E>(E enumValue)
            where E : struct
        {
            var type = typeof(E);
            var value = enumValue.ToString();

            var name =
                "{0}/{1}".Fmt(type.Name, value);

            return name;
        }

        private static string make_funnel_id(string name, string tag)
        {
            if (tag.IsEmpty())
                return name;

            return
                "{0}/{1}".Fmt(name, tag);
        }

        private IFunnel make_funnel(string name, string tag)
        {
            var funnelId = make_funnel_id(name, tag);

            var store = _repo.GetByFunnelId(funnelId);

            if (store == null)
            {
                store = _factory.MakeStore(funnelId);
                store = _repo.Add(funnelId, store);
            }

            var funnel = Funnel.New(this, store);

            _cache.AddInstance(funnel, store);

            return funnel;
        }

        private IFunnel get_funnel(string name, string tag)
        {
            return
                make_funnel(name, tag);
        }

        #endregion

        #region IFunnelsManager

        IFunnelsManager IFunnelsManager.Start()
        {
            var self = this as IFunnelsManagerEx;
            _idler.Repeat(_config.Config.Client.CleanDeadFunnelsSpan, self.CleanDeadFunnels);

            _agent.Start();
            _idler.Start();
            _threads.Start();

            return this;
        }

        void IFunnelsManager.Stop()
        {
            _idler.Stop();
            _agent.Stop();

            var stores = _repo.GetAllStores();

            if (stores != null)
            foreach (var store in stores)
            {
                store.Stop();
            }
        }

        void IFunnelsManagerEx.CleanDeadFunnels()
        {
            var stores = _cache.GetDeadInstances(_config.Config.Client.FunnelStoreTtlSpan);

            if (stores != null)
            foreach (var store in stores)
            {
                _repo.Remove(store);
            }

            var now = DateTime.UtcNow;
            var toBeStopped = _repo.PopRemovedStores(now, _config.Config.Client.FunnelStoreTtlSpan);

            foreach (var store in toBeStopped)
            {
                store.Stop();
            }
        }

        void IFunnelsManagerEx.PruneFunnelData(Guid storeId, string path)
        {
            IFunnelStore store = _repo.GetByStoreId(storeId);

            if (store == null)
                return;

            store.PruneFunnelData(path);
        }

        void IFunnelsManagerEx.ShopIsRemoved(comm.ShopInfo shopInfo)
        {
            throw new NotImplementedException();
        }

        bool IFunnelsManagerEx.ProcessIo()
        {
            return
                _sink.ProcessIo();
        }

        IFunnel IFunnelsManager.GetFunnelOfInstance<T>(T instance)
        {
            throw new NotImplementedException();
        }

        IFunnel IFunnelsManager.GetFunnelOfClass(Type type)
        {
            var name = type_to_name(type);
            return
                make_funnel(name, null);
        }

        IFunnel IFunnelsManager.GetCurrentFunnel()
        {
            return
                _current.Value;
        }

        IFunnel IFunnelsManager.SetCurrentFunnel(IFunnel funnel)
        {
            var prev = _current.Value;
            _current.Value = funnel;

            return prev;
        }

        #endregion
    }
}