using System;
using L4p.Common.Extensions;
using L4p.Common.FunnelsModel.client;
using L4p.Common.FunnelsModel.comm;
using L4p.Common.Helpers;

namespace L4p.Common.FunnelsModel
{
    public partial class Funnel : IFunnel
    {
        #region members

        private readonly string _funnelId;
        private readonly IFunnelsManager _funnels;
        private readonly IFunnelStore _store;
        private readonly IJsonPort _serialize;

        #endregion

        #region construction

        internal static IFunnel New(FunnelsManager asFriend, IFunnelStore store)
        {
            Validate.NotNull(asFriend);

            return
                new Funnel(asFriend, store);
        }

        private Funnel(IFunnelsManager funnels, IFunnelStore store)
        {
            _funnelId = store.FunnelId;
            _funnels = funnels;
            _store = store;
            _serialize = JsonPort.New();
        }

        #endregion

        #region private

        private string read_json(string tag)
        {
            var path = FunnelHelpers.MakePath(_funnelId, tag);

            var post = _store.GetPost(path);

            if (post == null)
                return null;

            return post.Json;
        }

        private void write_json(string tag, string json)
        {
            var path = FunnelHelpers.MakePath(_funnelId, tag);

            var post = new Post
            {
                FunnelId = _funnelId,
                Path = path, 
                Tag = tag,
                Json = json
            };

            _store.PublishPost(post);
        }

        private T get_by_type<T>()
        {
            Type type = typeof(T);

            string json = read_json(type.Name);

            if (json.IsEmpty())
                return default(T);

            var value = _serialize.FromJson<T>(json);

            return value;
        }

        #endregion

        #region IFunnel

        void IIfFunnel.Publish<T>(T value)
        {
            Type type = typeof(T);

            var json = _serialize.ToReadableJson(value);
            write_json(type.Name, json);
        }

        void IIfFunnel.Publish<T>(string tag, T value)
        {
            var json = _serialize.ToReadableJson(value);
            write_json(tag, json);
        }

        void IIfFunnel.Publish<E, T>(E tag, T value)
        {
            var json = _serialize.ToReadableJson(value);
            write_json(tag.ToString(), json);
        }

        IFunnelLogger IIfFunnel.Log
        {
            get { return Null.Logger; }
        }

        IFunnelCounters IIfFunnel.Count
        {
            get { return Null.Counters; }
        }

        T IFunnel.Get<T>()
        {
            return
                get_by_type<T>();
        }

        T IFunnel.Get<T>(T likeMe)
        {
            return
                get_by_type<T>();
        }

        T IFunnel.Get<T>(string tag)
        {
            string json = read_json(tag);

            if (json.IsEmpty())
                return default(T);

            var value = _serialize.FromJson<T>(json);

            return value;
        }

        T IFunnel.Get<E, T>(E tag)
        {
            string json = read_json(tag.ToString());

            if (json.IsEmpty())
                return default(T);

            var value = _serialize.FromJson<T>(json);

            return value;
        }

        IFunnelTags IFunnel.Tags
        {
            get { return Null.Tags; }
        }

        IFunnelTimeline IFunnel.Timeline
        {
            get { return Null.Timeline; }
        }

        string IFunnel.Id
        {
            get { return _funnelId; }
        }

        IIfFunnel IFunnel.If(bool expr)
        {
            if (!expr)
                return Null.IfFunnel;

            return this;
        }

        IIfFunnel IFunnel.If(string str)
        {
            if (str.IsEmpty())
                return Null.IfFunnel;

            return this;
        }

        IIfFunnel IFunnel.If<T>(T obj)
        {
            if (obj == null)
                return Null.IfFunnel;

            return this;
        }

        void IFunnel.WithMe(Action action)
        {
            var prev = _funnels.SetCurrentFunnel(this);

            try
            {
                action();
            }
            finally
            {
                _funnels.SetCurrentFunnel(prev);
            }
        }

        R IFunnel.WithMe<R>(Func<R> func)
        {
            var prev = _funnels.SetCurrentFunnel(this);

            try
            {
                return
                    func();
            }
            finally
            {
                _funnels.SetCurrentFunnel(prev);
            }
        }

        #endregion

        #region IDisposable

        class WithMeGuard : IDisposable
        {
            private readonly IFunnelsManager _funnels;
            private readonly IFunnel _prev ;

            public WithMeGuard(IFunnelsManager funnels, IFunnel funnel)
            {
                _funnels = funnels;
                _prev = _funnels.SetCurrentFunnel(funnel);
            }

            void IDisposable.Dispose()
            {
                _funnels.SetCurrentFunnel(_prev);
            }
        }

        IDisposable IFunnel.WithMe()
        {
            return
                new WithMeGuard(_funnels, this);
        }

        #endregion
    }
}