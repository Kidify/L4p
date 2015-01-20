using System;
using System.Collections.Generic;

namespace L4p.Common.FunnelsModel.hub
{
    interface IHubCache
    {
        Guid[] PostIsPublished(Guid storeId, string path);
        void PostIsRetrieved(Guid storeId, string path);
        void PathIsRemoved(string path);
    }

    class HubCache : IHubCache
    {
        #region members

        private readonly Dictionary<string, List<Guid>> _repo;

        #endregion

        #region construction

        public static IHubCache New()
        {
            return
                new HubCache();
        }

        private HubCache()
        {
            _repo = new Dictionary<string, List<Guid>>();
        }

        #endregion

        #region private

        List<Guid> get_list_of_clean_agents(string path)
        {
            List<Guid> agents;

            if (!_repo.TryGetValue(path, out agents))
            {
                agents = new List<Guid>();
                _repo.Add(path, agents);
            }

            return agents;
        }

        #endregion

        #region IHubCache

        Guid[] IHubCache.PostIsPublished(Guid storeId, string path)
        {
            var cleanAgents = get_list_of_clean_agents(path);
            Guid[] listeners = cleanAgents.ToArray();

            cleanAgents.Clear();
            cleanAgents.Add(storeId);

            return listeners;
        }

        void IHubCache.PostIsRetrieved(Guid storeId, string path)
        {
            var cleanAgents = get_list_of_clean_agents(path);
            cleanAgents.Add(storeId);
        }

        void IHubCache.PathIsRemoved(string path)
        {
            _repo.Remove(path);
        }

        #endregion
    }

    class SynchedHubCache : IHubCache
    {
        private readonly IHubCache _impl;

        public static IHubCache New(IHubCache impl) { return new SynchedHubCache(impl); }
        private SynchedHubCache(IHubCache impl) { _impl = impl; }

        Guid[] IHubCache.PostIsPublished(Guid storeId, string path) { lock(this) return _impl.PostIsPublished(storeId, path); }
        void IHubCache.PostIsRetrieved(Guid storeId, string path) { lock(this) _impl.PostIsRetrieved(storeId, path); }
        void IHubCache.PathIsRemoved(string path) { lock(this) _impl.PathIsRemoved(path); }
    }
}