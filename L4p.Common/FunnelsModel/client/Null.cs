using System;
using System.Dynamic;
using L4p.Common.FunnelsModel.comm;

namespace L4p.Common.FunnelsModel.client
{
    static class Null
    {
        public static readonly NullFunnel Funnel = new NullFunnel();
        public static readonly NullIfFunnel IfFunnel = new NullIfFunnel();
        public static readonly NullLogger Logger = new NullLogger();
        public static readonly NullCounters Counters = new NullCounters();
        public static readonly NullTags Tags = new NullTags();
        public static readonly NullTimeline Timeline = new NullTimeline();
        public static readonly NullStore Store = new NullStore();
        public static readonly NullShop Shop = new NullShop();
        public static readonly NullDynamic Dynamic = new NullDynamic();
        public static readonly NullDisposable Disposable = new NullDisposable();
    }

    class NullCounters : IFunnelCounters
    {
        dynamic IFunnelCounters.Hit                             { get { return Null.Dynamic; }}
        dynamic IFunnelCounters.Time                            { get { return Null.Dynamic; }}
    }

    class NullIfFunnel : IIfFunnel
    {
        void IIfFunnel.Publish<T>(T value)                      {}
        void IIfFunnel.Publish<T>(string tag, T value)          {}
        void IIfFunnel.Publish<E, T>(E tag, T value)            {}
        IFunnelLogger IIfFunnel.Log                             { get { return Null.Logger; } }
        IFunnelCounters IIfFunnel.Count                         { get { return Null.Counters; } }
    }

    class NullFunnel : NullIfFunnel, IFunnel
    {
        T IFunnel.Get<T>()                                      { return default(T); }
        T IFunnel.Get<T>(T likeMe)                              { return default(T); }
        T IFunnel.Get<T>(string tag)                            { return default(T); }
        T IFunnel.Get<E, T>(E tag)                              { return default(T); }
        IFunnelTags IFunnel.Tags                                { get { return Null.Tags; } }
        IFunnelTimeline IFunnel.Timeline                        { get { return Null.Timeline; } }
        string IFunnel.Id                                       { get { return ""; } }
        IIfFunnel IFunnel.If(bool expr)                         { return Null.IfFunnel; }
        IIfFunnel IFunnel.If(string str)                        { return Null.IfFunnel; }
        IIfFunnel IFunnel.If<T>(T obj)                          { return Null.IfFunnel; }
        IDisposable IFunnel.WithMe()                            { return Null.Disposable; }
        void IFunnel.WithMe(Action action)                      { action(); }
        R IFunnel.WithMe<R>(Func<R> func)                       { return func(); }
    }

    class NullLogger : IFunnelLogger
    {
        dynamic IFunnelLogger.Error                             { get { return Null.Dynamic; }}
        dynamic IFunnelLogger.Warn                              { get { return Null.Dynamic; }}
        dynamic IFunnelLogger.Info                              { get { return Null.Dynamic; }}
        dynamic IFunnelLogger.Trace                             { get { return Null.Dynamic; }}
    }

    class NullTags : IFunnelTags
    {
        void IFunnelTags.Add(int tag)                           {}
        void IFunnelTags.Add(uint tag)                          {}
        void IFunnelTags.Add(long tag)                          {}
        void IFunnelTags.Add(string tag)                        {}
        void IFunnelTags.Add(Guid tag)                          {}
    }

    class NullTimeline : IFunnelTimeline
    {
        T IFunnelTimeline.GetAt<T>(DateTime at)                 { return default(T); }
        void IFunnelTimeline.StoreAt<T>(T data, DateTime at)    {}
    }

    class NullStore : IFunnelStore
    {
        Post IFunnelStore.GetPost(string path)                  { return null; }
        void IFunnelStore.PublishPost(Post post)                { ; }
        void IFunnelStore.PruneFunnelData(string path)          { ; }
        void IFunnelStore.Stop()                                { ; }

        Guid IFunnelStore.StoreId                               { get { return Guid.Empty; } }
        string IFunnelStore.FunnelId                            { get; set; }
        DateTime IFunnelStore.DeadAt                            { get; set; }
    }

    class NullShop : IFunnelsShop
    {
        void IFunnelsShop.PublishPost(Guid storeId, Post post)  { }
        Post IFunnelsShop.GetPost(Guid storeId, string path)    { return null; }
        void IFunnelsShop.StoreIsRemoved(Guid storeId)          { }
        void IFunnelsShop.CloseConnection()                     { }
    }

    class NullDisposable : IDisposable
    {
        void IDisposable.Dispose()                              { ; }
    }

    class NullDynamic : DynamicObject
    {
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out Object result)
        {
            result = this;
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = this;
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            result = this;
            return true;
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            result = this;
            return true;
        }
    }
}