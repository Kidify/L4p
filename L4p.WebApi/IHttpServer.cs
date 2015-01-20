using System;
using System.Web;

namespace L4p.WebApi
{
    public delegate void RouteHandler(HttpContext http);
    public delegate void AsyncRouteHandler(HttpContext http, Action notifyCompletion);

    public interface IHttpServer
    {
        SingleRoute AddRoute(string path, IBlController controller, RouteHandler handler);
        SingleRoute AddRoute(string path, IBlController controller, AsyncRouteHandler handler);
        void ConfigureThrottling(IBusinessLogicModule listener, ThrottlingConfig config);
        void SetWhiteIpList(IBusinessLogicModule listener, IpList list);
    }
}