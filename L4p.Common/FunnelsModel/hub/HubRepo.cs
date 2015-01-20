using System;
using System.Collections.Generic;
using L4p.Common.Concerns;
using L4p.Common.FunnelsModel.comm;

namespace L4p.Common.FunnelsModel.hub
{
    interface IHubRepo
    {
        void AddAgent(Guid storeId, IFunnelsAgent agent);
        IFunnelsAgent GetAgent(Guid storeId);
        void RemoveAgent(Guid storeId);

        void AddPost(string path, Post post);
        Post GetPost(string path);
        Post GetPostAt(string path, DateTime at);
        Post[] GetPosts(string path, DateTime from, DateTime to);
    }

    class HubRepo : IHubRepo
    {
        #region members

        private readonly Dictionary<Guid, IFunnelsAgent> _agents;
        private readonly Dictionary<string, Post> _posts;

        #endregion

        #region construction

        public static IHubRepo New()
        {
            return
                new HubRepo();
        }

        private HubRepo()
        {
            _agents = new Dictionary<Guid, IFunnelsAgent>();
            _posts = new Dictionary<string, Post>();
        }

        #endregion

        #region private
        #endregion

        #region IHubRepo

        void IHubRepo.AddAgent(Guid storeId, IFunnelsAgent agent)
        {
            _agents[storeId] = agent;
        }

        IFunnelsAgent IHubRepo.GetAgent(Guid storeId)
        {
            IFunnelsAgent agent;

            _agents.TryGetValue(storeId, out agent);
            return agent;
        }

        void IHubRepo.RemoveAgent(Guid storeId)
        {
            _agents.Remove(storeId);
        }

        void IHubRepo.AddPost(string path, Post post)
        {
            _posts[path] = post;
        }

        Post IHubRepo.GetPost(string path)
        {
            Post post;

            _posts.TryGetValue(path, out post);
            return post;
        }

        Post IHubRepo.GetPostAt(string path, DateTime at)
        {
            throw new NotImplementedException();
        }

        Post[] IHubRepo.GetPosts(string path, DateTime @from, DateTime to)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    class MReadSWriteHubRepo : ManyReadersSingleWriter<IHubRepo>, IHubRepo
    {
        public static IHubRepo New(IHubRepo impl) { return new MReadSWriteHubRepo(impl); }
        private MReadSWriteHubRepo(IHubRepo impl) : base(impl) { }

        void IHubRepo.AddAgent(Guid storeId, IFunnelsAgent agent) { using_write_lock(() => _impl.AddAgent(storeId, agent)); }
        IFunnelsAgent IHubRepo.GetAgent(Guid storeId) { return using_read_lock(() => _impl.GetAgent(storeId)); }
        void IHubRepo.RemoveAgent(Guid storeId) { using_write_lock(() => _impl.RemoveAgent(storeId)); }

        void IHubRepo.AddPost(string path, Post post) { using_write_lock(() => _impl.AddPost(path, post)); }
        Post IHubRepo.GetPost(string path) { return using_read_lock(() => _impl.GetPost(path)); }
        Post IHubRepo.GetPostAt(string path, DateTime at) { return using_read_lock(() => _impl.GetPostAt(path, at)); }
        Post[] IHubRepo.GetPosts(string path, DateTime from, DateTime to) { return using_read_lock(() => _impl.GetPosts(path, from, to)); }
    }
}