using System;
using L4p.Common.FunnelsModel.config;
using L4p.Common.Loggers;

namespace L4p.Common.FunnelsModel
{
    public static class Funnels
    {
        private static IFunnelsManager _funnels = NotInitializedFunnels.New();

        public static IFunnelsManager Instance
        {
            get { return _funnels; }
            set { _funnels = value; }
        }

        public static IFunnel GetFunnelBy(string name)                                  { return _funnels.GetFunnelBy(name); }
        public static IFunnel GetFunnelBy<E>(E enumValue) where E : struct              { return _funnels.GetFunnelBy(enumValue); }

        public static IFunnel GetFunnelBy(string name, string tag)                      { return _funnels.GetFunnelBy(name, tag); }
        public static IFunnel GetFunnelBy(string name, int tag)                         { return _funnels.GetFunnelBy(name, tag); }
        public static IFunnel GetFunnelBy(string name, long tag)                        { return _funnels.GetFunnelBy(name, tag); }
        public static IFunnel GetFunnelBy(string name, Guid tag)                        { return _funnels.GetFunnelBy(name, tag); }

        public static IFunnel GetFunnelBy<T>(string tag) where T : class                { return _funnels.GetFunnelBy<T>(tag); }
        public static IFunnel GetFunnelBy<T>(int tag) where T : class                   { return _funnels.GetFunnelBy<T>(tag); }
        public static IFunnel GetFunnelBy<T>(long tag) where T : class                  { return _funnels.GetFunnelBy<T>(tag); }
        public static IFunnel GetFunnelBy<T>(Guid tag) where T : class                  { return _funnels.GetFunnelBy<T>(tag); }

        public static IFunnel GetFunnelBy<E>(E enumValue, string tag) where E : struct  { return _funnels.GetFunnelBy(enumValue, tag); }
        public static IFunnel GetFunnelBy<E>(E enumValue, int tag) where E : struct     { return _funnels.GetFunnelBy(enumValue, tag); }
        public static IFunnel GetFunnelBy<E>(E enumValue, long tag) where E : struct    { return _funnels.GetFunnelBy(enumValue, tag); }
        public static IFunnel GetFunnelBy<E>(E enumValue, Guid tag) where E : struct    { return _funnels.GetFunnelBy(enumValue, tag); }

        public static IFunnel NewFunnel(string name)                                    { return _funnels.NewFunnel(name); }
        public static IFunnel NewFunnel(string name, string tag)                        { return _funnels.NewFunnel(name, tag); }
        public static IFunnel NewFunnel(string name, int tag)                           { return _funnels.NewFunnel(name, tag); }
        public static IFunnel NewFunnel(string name, long tag)                          { return _funnels.NewFunnel(name, tag); }
        public static IFunnel NewFunnel(string name, Guid tag)                          { return _funnels.NewFunnel(name, tag); }

        public static IFunnel NewFunnel<T>() where T : class                            { return _funnels.NewFunnel<T>(); }
        public static IFunnel NewFunnel<T>(string tag) where T : class                  { return _funnels.NewFunnel<T>(tag); }
        public static IFunnel NewFunnel<T>(int tag) where T : class                     { return _funnels.NewFunnel<T>(tag); }
        public static IFunnel NewFunnel<T>(long tag) where T : class                    { return _funnels.NewFunnel<T>(tag); }
        public static IFunnel NewFunnel<T>(Guid tag) where T : class                    { return _funnels.NewFunnel<T>(tag); }

        public static IFunnel NewFunnel<E>(E enumValue) where E : struct                { return _funnels.NewFunnel(enumValue); }
        public static IFunnel NewFunnel<E>(E enumValue, string tag) where E : struct    { return _funnels.NewFunnel(enumValue, tag); }
        public static IFunnel NewFunnel<E>(E enumValue, int tag) where E : struct       { return _funnels.NewFunnel(enumValue, tag); }
        public static IFunnel NewFunnel<E>(E enumValue, long tag) where E : struct      { return _funnels.NewFunnel(enumValue, tag); }
        public static IFunnel NewFunnel<E>(E enumValue, Guid tag) where E : struct      { return _funnels.NewFunnel(enumValue, tag); }

        public static IFunnel GetFunnelOfInstance<T>(T instance) where T : class        { return _funnels.GetFunnelOfInstance(instance); }
        public static IFunnel GetFunnelOfClass(Type type)                               { return _funnels.GetFunnelOfClass(type); }
            
        public static IFunnel GetCurrentFunnel()                                        { return _funnels.GetCurrentFunnel(); }
        public static IFunnel SetCurrentFunnel(IFunnel funnel)                          { return _funnels.SetCurrentFunnel(funnel); }

        public static void Stop()                                                       { _funnels.Stop(); }
    }
}