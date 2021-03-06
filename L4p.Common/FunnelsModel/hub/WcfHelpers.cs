using System;
using System.ServiceModel.Channels;
using L4p.Common.FunnelsModel.comm;
using L4p.Common.Loggers;
using L4p.Common.Wcf;

namespace L4p.Common.FunnelsModel.hub
{
    public static class WcfHelpers
    {
        public static IDisposable AsServiceAt(this IFunnelsResolver target, ILogFile log, string uri, Binding binding = null)
        {
            var wcf = hub.wcf.FunnelsResolver.New(target);

            var host = WcfHost<IFunnelsResolver>.New(log, wcf);
            host.StartAt(uri, binding);

            return host;
        }

        public static IDisposable AsService(this IFunnelsShop target, ILogFile log, Binding binding = null)
        {
            var info = target.GetInfo();
            var uri = info.Uri;

            var wcf = hub.wcf.FunnelsShop.New(target);

            var host = WcfHost<IFunnelsShop>.New(log, wcf);
            host.StartAt(uri, binding);

            return host;
        }
    }
}