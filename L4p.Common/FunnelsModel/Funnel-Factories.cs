using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace L4p.Common.FunnelsModel
{
    public partial class Funnel
    {
        public static IFunnel GetBy(string name)                                    { return Funnels.GetFunnelBy(name); }
        public static IFunnel GetBy<E>(E enumValue) where E : struct                { return Funnels.GetFunnelBy(enumValue); }

        public static IFunnel GetBy(string name, string tag)                        { return Funnels.GetFunnelBy(name, tag); }
        public static IFunnel GetBy(string name, int tag)                           { return Funnels.GetFunnelBy(name, tag); }
        public static IFunnel GetBy(string name, long tag)                          { return Funnels.GetFunnelBy(name, tag); }
        public static IFunnel GetBy(string name, Guid tag)                          { return Funnels.GetFunnelBy(name, tag); }

        public static IFunnel GetBy<T>(string tag) where T : class                  { return Funnels.GetFunnelBy<T>(tag); }
        public static IFunnel GetBy<T>(int tag) where T : class                     { return Funnels.GetFunnelBy<T>(tag); }
        public static IFunnel GetBy<T>(long tag) where T : class                    { return Funnels.GetFunnelBy<T>(tag); }
        public static IFunnel GetBy<T>(Guid tag) where T : class                    { return Funnels.GetFunnelBy<T>(tag); }

        public static IFunnel GetBy<E>(E enumValue, string tag) where E : struct    { return Funnels.GetFunnelBy(enumValue, tag); }
        public static IFunnel GetBy<E>(E enumValue, int tag) where E : struct       { return Funnels.GetFunnelBy(enumValue, tag); }
        public static IFunnel GetBy<E>(E enumValue, long tag) where E : struct      { return Funnels.GetFunnelBy(enumValue, tag); }
        public static IFunnel GetBy<E>(E enumValue, Guid tag) where E : struct      { return Funnels.GetFunnelBy(enumValue, tag); }

        public static IFunnel New(string name)                                      { return Funnels.NewFunnel(name); }
        public static IFunnel New(string name, string tag)                          { return Funnels.NewFunnel(name, tag); }
        public static IFunnel New(string name, int tag)                             { return Funnels.NewFunnel(name, tag); }
        public static IFunnel New(string name, long tag)                            { return Funnels.NewFunnel(name, tag); }
        public static IFunnel New(string name, Guid tag)                            { return Funnels.NewFunnel(name, tag); }

        public static IFunnel New<T>() where T : class                              { return Funnels.NewFunnel<T>(); }
        public static IFunnel New<T>(string tag ) where T : class                   { return Funnels.NewFunnel<T>(tag); }
        public static IFunnel New<T>(int tag) where T : class                       { return Funnels.NewFunnel<T>(tag); }
        public static IFunnel New<T>(long tag) where T : class                      { return Funnels.NewFunnel<T>(tag); }
        public static IFunnel New<T>(Guid tag) where T : class                      { return Funnels.NewFunnel<T>(tag); }

        public static IFunnel New<E>(E enumValue) where E : struct                  { return Funnels.NewFunnel(enumValue); }
        public static IFunnel New<E>(E enumValue, string tag) where E : struct      { return Funnels.NewFunnel(enumValue, tag); }
        public static IFunnel New<E>(E enumValue, int tag) where E : struct         { return Funnels.NewFunnel(enumValue, tag); }
        public static IFunnel New<E>(E enumValue, long tag) where E : struct        { return Funnels.NewFunnel(enumValue, tag); }
        public static IFunnel New<E>(E enumValue, Guid tag) where E : struct        { return Funnels.NewFunnel(enumValue, tag); }

        public static IFunnel OfInstance<T>(T instance) where T : class
        {
            return 
                Funnels.GetFunnelOfInstance(instance);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static IFunnel OfThisClass()
        {
            Type type = new StackFrame(1).GetMethod().DeclaringType;

            return
                Funnels.GetFunnelOfClass(type);
        }

        public static IFunnel Current
        {
            get { return Funnels.GetCurrentFunnel(); }
        }
    }
}