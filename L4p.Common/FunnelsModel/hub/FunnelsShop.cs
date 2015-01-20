using System;
using L4p.Common.Extensions;
using L4p.Common.FunnelsModel.comm;
using L4p.Common.FunnelsModel.config;
using L4p.Common.IoCs;
using L4p.Common.Loggers;

namespace L4p.Common.FunnelsModel.hub
{
    public class FunnelsShop : IFunnelsShop
    {
        #region members

        private readonly ILogFile _log;
        private readonly IFmConfigRa _config;
        private readonly ShopInfo _info;
        private readonly IHubCache _cache;
        private readonly IHubRepo _repo;

        #endregion

        #region construction

        public static IFunnelsShop New(IIoC ioc, FunnelsConfig config)
        {
            return
                new FunnelsShop(ioc, config);
        }

        private FunnelsShop(IIoC ioc, FunnelsConfig config)
        {
            _log = ioc.Resolve<ILogFile>();
            _config = FmConfigRa.New(config);
            _info = make_info(_config.Config);
            _cache = SynchedHubCache.New(HubCache.New());
            _repo = MReadSWriteHubRepo.New(HubRepo.New());
        }

        #endregion

        #region private

        private static ShopInfo make_info(FunnelsConfig config)
        {
            var host = Environment.MachineName;
            var port = config.Port;
            var shopId = Guid.NewGuid();

            var uri = config.ShopUri.Fmt(host, port, shopId);

            var info = new ShopInfo
                {
                    ShopId = shopId,
                    Uri = uri
                };

            return info;
        }

        private void disconnect(IFunnelsAgent agent)
        {
            _repo.RemoveAgent(agent.AgentId);
        }

        private void prune_funnel_data(Guid storeId, string tag)
        {
            var agent = _repo.GetAgent(storeId);

            try
            {
                agent.PruneFunnelData(storeId, tag);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Failed to prune cache for '{0}/{1}'", storeId, tag);
                disconnect(agent);
            }
        }

        #endregion

        #region interface

        ShopInfo IFunnelsShop.GetInfo()
        {
            return _info;
        }

        ShopInfo IFunnelsShop.RegisterStore(StoreInfo info)
        {
            var agentInfo = new AgentInfo
                {
                    AgentId = info.AgentId,
                    Uri = info.AgentUri
                };

            var agent = FunnelsAgent.New(agentInfo);
            _repo.AddAgent(info.StoreId, agent);

            _log.Info("Funnel store '{0} ({1})' is registered at shop '{2}'", info.FunnelId, info.StoreId, info.ShopUri);

            return _info;
        }

        void IFunnelsShop.StoreIsRemoved(Guid storeId)
        {
        }

        void IFunnelsShop.PublishPost(Guid storeId, Post post)
        {
            _repo.AddPost(post.Path, post);
            var listeners = _cache.PostIsPublished(storeId, post.Path);

            foreach (var listener in listeners)
            {
                if (listener == storeId)    // do not prune data of a publishing agent
                    continue;

                prune_funnel_data(listener, post.Path);
            }
        }

        Post IFunnelsShop.GetPost(Guid storeId, string path)
        {
            var post = _repo.GetPost(path);
            _cache.PostIsRetrieved(storeId, path);

            return post;
        }

        void IFunnelsShop.KeepAlive()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}