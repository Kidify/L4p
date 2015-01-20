using System;

namespace L4p.Common.FunnelsModel
{
    public interface IIfFunnel
    {
        void Publish<T>(T value);
        void Publish<T>(string tag, T value);
        void Publish<E, T>(E tag, T value) where E : struct;

        IFunnelLogger Log { get; }
        IFunnelCounters Count { get; }
    }

    public interface IFunnel : IIfFunnel
    {
        T Get<T>() where T : class;
        T Get<T>(T likeMe);

        T Get<T>(string tag) where T : class;

        T Get<E, T>(E tag) 
            where E : struct
            where T : class;

        IFunnelTags Tags { get; }
        IFunnelTimeline Timeline { get; }

        string Id { get; }

        IIfFunnel If(bool expr);
        IIfFunnel If(string str);
        IIfFunnel If<T>(T obj) where T : class;

        IDisposable WithMe();
        void WithMe(Action action);
        R WithMe<R>(Func<R> func);
    }
}