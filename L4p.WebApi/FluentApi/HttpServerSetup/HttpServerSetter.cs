using System;
using System.Diagnostics;

namespace L4p.WebApi.FluentApi.HttpServerSetup
{
    class HttpServerSetter : IHttpServerSetup, 
        IMapRoute, IRequestSpecifier, IPostRequest, IGetRequest
    {
        class SetterContext
        {
            public string Route { get; set; }
            public string ParameterAs { get; set; }
        }

        #region members

        private readonly IHttpServer _server;
        private readonly SetterContext _props;

        private SingleRoute _route;

        #endregion

        #region construction

        public static IHttpServerSetup New(IHttpServer server)
        {
            return 
                new HttpServerSetter(server);
        }

        private HttpServerSetter(IHttpServer server)
        {
            _server = server;
            _props = new SetterContext();
        }

        #endregion

        #region IMapTo
        IRequestSpecifier IMapRoute.MapTo(IBlController listener, RouteHandler handler)
        {
            _route = _server.AddRoute(_props.Route, listener, handler);
            return this;
        }
        #endregion

        #region IAnyRequest
        IPostRequest IRequestSpecifier.AsPost()
        {
            Trace.Assert(_route != null);

            _route.IsPostRequest = true;
            _route.IsGetRequest = false;

            return this;
        }
        IGetRequest IRequestSpecifier.AsGet()
        {
            Trace.Assert(_route != null);

            _route.IsPostRequest = false;
            _route.IsGetRequest = true;

            return this;
        }
        IGetRequest IRequestSpecifier.AsGetJsonP()
        {
            Trace.Assert(_route != null);

            _route.IsPostRequest = false;
            _route.IsGetRequest = true;

            return this;
        }
        IGetRequest IRequestSpecifier.AsGetPng()
        {
            Trace.Assert(_route != null);

            _route.IsPostRequest = false;
            _route.IsGetRequest = true;

            return this;
        }
        IMapRoute IRequestSpecifier.Route(string route)
        {
            _props.Route = route;
            return this;
        }
        #endregion

        #region IPostRequest
        IMapRoute IPostRequest.Route(string route)
        {
            _props.Route = route;
            return this;
        }
        #endregion

        #region IGetRequest
        IMapRoute IGetRequest.Route(string route)
        {
            _props.Route = route;
            return this;
        }
        #endregion

        #region IHttpServerSetup
        IMapRoute IHttpServerSetup.Route(string route)
        {
            _props.Route = route;
            return this;
        }
        #endregion
    }
}