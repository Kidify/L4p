using System;

namespace L4p.Common.FunnelsModel
{
    public partial interface IFunnelsManager
    {
        IFunnel GetFunnelBy(string name);
        IFunnel GetFunnelBy<E>(E enumValue) where E : struct;

        IFunnel GetFunnelBy(string name, string tag);
        IFunnel GetFunnelBy(string name, int tag);
        IFunnel GetFunnelBy(string name, long tag);
        IFunnel GetFunnelBy(string name, Guid tag);

        IFunnel GetFunnelBy<T>(string tag) where T : class;
        IFunnel GetFunnelBy<T>(int tag) where T : class;
        IFunnel GetFunnelBy<T>(long tag) where T : class;
        IFunnel GetFunnelBy<T>(Guid tag) where T : class;

        IFunnel GetFunnelBy<E>(E enumValue, string tag) where E : struct;
        IFunnel GetFunnelBy<E>(E enumValue, int tag) where E : struct;
        IFunnel GetFunnelBy<E>(E enumValue, long tag) where E : struct;
        IFunnel GetFunnelBy<E>(E enumValue, Guid tag) where E : struct;

        IFunnel NewFunnel(string name);
        IFunnel NewFunnel(string name, string tag);
        IFunnel NewFunnel(string name, int tag);
        IFunnel NewFunnel(string name, long tag);
        IFunnel NewFunnel(string name, Guid tag);

        IFunnel NewFunnel<T>() where T : class;
        IFunnel NewFunnel<T>(string tag) where T : class;
        IFunnel NewFunnel<T>(int tag) where T : class;
        IFunnel NewFunnel<T>(long tag) where T : class;
        IFunnel NewFunnel<T>(Guid tag) where T : class;

        IFunnel NewFunnel<E>(E enumValue) where E : struct;
        IFunnel NewFunnel<E>(E enumValue, string tag) where E : struct;
        IFunnel NewFunnel<E>(E enumValue, int tag) where E : struct;
        IFunnel NewFunnel<E>(E enumValue, long tag) where E : struct;
        IFunnel NewFunnel<E>(E enumValue, Guid tag) where E : struct;
    }

    partial class FunnelsManager
    {
        #region IFunnelsManager

        IFunnel IFunnelsManager.GetFunnelBy(string name)
        {
            return
                get_funnel(name, null);
        }

        IFunnel IFunnelsManager.GetFunnelBy<E>(E enumValue)
        {
            var name = enum_to_name(enumValue);
            return
                get_funnel(name, null);
        }

        IFunnel IFunnelsManager.GetFunnelBy(string name, string tag)
        {
            return
                get_funnel(name, tag);
        }

        IFunnel IFunnelsManager.GetFunnelBy(string name, int tag)
        {
            return
                get_funnel(name, tag.ToString());
        }

        IFunnel IFunnelsManager.GetFunnelBy(string name, long tag)
        {
            return
                get_funnel(name, tag.ToString());
        }

        IFunnel IFunnelsManager.GetFunnelBy(string name, Guid tag)
        {
            return
                get_funnel(name, tag.ToString("N"));
        }

        IFunnel IFunnelsManager.GetFunnelBy<T>(string tag)
        {
            var name = type_to_name(typeof(T));
            return
                get_funnel(name, tag);
        }

        IFunnel IFunnelsManager.GetFunnelBy<T>(int tag)
        {
            var name = type_to_name(typeof(T));
            return
                get_funnel(name, tag.ToString());
        }

        IFunnel IFunnelsManager.GetFunnelBy<T>(long tag)
        {
            var name = type_to_name(typeof(T));
            return
                get_funnel(name, tag.ToString());
        }

        IFunnel IFunnelsManager.GetFunnelBy<T>(Guid tag)
        {
            var name = type_to_name(typeof(T));
            return
                get_funnel(name, tag.ToString("N"));
        }

        IFunnel IFunnelsManager.GetFunnelBy<E>(E enumValue, string tag)
        {
            var name = enum_to_name(enumValue);
            return
                get_funnel(name, tag);
        }

        IFunnel IFunnelsManager.GetFunnelBy<E>(E enumValue, int tag)
        {
            var name = enum_to_name(enumValue);
            return
                get_funnel(name, tag.ToString());
        }

        IFunnel IFunnelsManager.GetFunnelBy<E>(E enumValue, long tag)
        {
            var name = enum_to_name(enumValue);
            return
                get_funnel(name, tag.ToString());
        }

        IFunnel IFunnelsManager.GetFunnelBy<E>(E enumValue, Guid tag)
        {
            var name = enum_to_name(enumValue);
            return
                get_funnel(name, tag.ToString("N"));
        }

        IFunnel IFunnelsManager.NewFunnel(string name)
        {
            return
                make_funnel(name, null);
        }

        IFunnel IFunnelsManager.NewFunnel(string name, string tag)
        {
            return
                make_funnel(name, tag);
        }

        IFunnel IFunnelsManager.NewFunnel(string name, int tag)
        {
            return
                make_funnel(name, tag.ToString());
        }

        IFunnel IFunnelsManager.NewFunnel(string name, long tag)
        {
            return
                make_funnel(name, tag.ToString());
        }

        IFunnel IFunnelsManager.NewFunnel(string name, Guid tag)
        {
            return
                make_funnel(name, tag.ToString("N"));
        }

        IFunnel IFunnelsManager.NewFunnel<T>()
        {
            var name = type_to_name(typeof(T));
            return
                make_funnel(name, null);
        }

        IFunnel IFunnelsManager.NewFunnel<T>(string tag)
        {
            var name = type_to_name(typeof(T));
            return
                make_funnel(name, tag);
        }

        IFunnel IFunnelsManager.NewFunnel<T>(int tag)
        {
            var name = type_to_name(typeof(T));
            return
                make_funnel(name, tag.ToString());
        }

        IFunnel IFunnelsManager.NewFunnel<T>(long tag)
        {
            var name = type_to_name(typeof(T));
            return
                make_funnel(name, tag.ToString());
        }

        IFunnel IFunnelsManager.NewFunnel<T>(Guid tag)
        {
            var name = type_to_name(typeof(T));
            return
                make_funnel(name, tag.ToString("N"));
        }

        IFunnel IFunnelsManager.NewFunnel<E>(E enumValue)
        {
            var name = enum_to_name(enumValue);
            return
                make_funnel(name, null);
        }

        IFunnel IFunnelsManager.NewFunnel<E>(E enumValue, string tag)
        {
            var name = enum_to_name(enumValue);
            return
                make_funnel(name, tag);
        }

        IFunnel IFunnelsManager.NewFunnel<E>(E enumValue, int tag)
        {
            var name = enum_to_name(enumValue);
            return
                make_funnel(name, tag.ToString());
        }

        IFunnel IFunnelsManager.NewFunnel<E>(E enumValue, long tag)
        {
            var name = enum_to_name(enumValue);
            return
                make_funnel(name, tag.ToString());
        }

        IFunnel IFunnelsManager.NewFunnel<E>(E enumValue, Guid tag)
        {
            var name = enum_to_name(enumValue);
            return
                make_funnel(name, tag.ToString("N"));
        }

        #endregion
    }
}