using System;
using System.Collections.Generic;
using System.Web;
using System.Diagnostics;
using L4p.Common;
using L4p.Common.Extensions;
using L4p.Common.Helpers;
using L4p.Common.Loggers;

namespace L4p.WebApi
{
    public delegate void ModuleEntryPoint(IWebApiController controller);
    public delegate RequestConfig MakeRequestConfigHandler(HttpContext http);
    public delegate IBlController ModuleFactory(string moduleName, ILogFile log);
    
    public interface IWebApiController
    {
        SingleRoute GetRoute(string path);
        void ProcessRequest(HttpContext http, SingleRoute route, Action notifyCompletion);
        void InitializeModule(string name, ModuleFactory makeNewModule);
        RequestConfig GetRequestConfig(HttpContext http);
        void Shut();
    }

    public class WebApiController 
        : IWebApiController, IHttpServer
    {
        #region members

        private readonly IRouteRepository _routes;
        private readonly MakeRequestConfigHandler _makeRequestConfig;
        private readonly ILogFile _log;
        private readonly List<IBlController> _modules;

        private string _initializationFailureMsg;

        #endregion

        #region construction

        public static IWebApiController New(MakeRequestConfigHandler makeRequestConfig = null)
        {
            return
                new WebApiController(makeRequestConfig);
        }

        private WebApiController(MakeRequestConfigHandler makeRequestConfig)
        {
            _routes = RouteRepository.New();
            _makeRequestConfig = makeRequestConfig;
            _log = LogFile.New("web_api_controller.log");
            _modules = new List<IBlController>();
        }

        #endregion

        #region private

        private SingleRoute find_route(string path)
        {
            var route = _routes.FindRoute(path);

            if (route == null)
            {
                throw
                    new UriIsNotFoundException("Route '{0}' is not found", path);
            }

            return route;
        }

        private SingleRoute add_route(SingleRoute route)
        {
            return
                _routes.AddRoute(route);
        }

        private void invoke_handler(HttpContext http, SingleRoute route, Action notifyCompletion)
        {
            try
            {
                route.Controller.ProcessRequest(route.Handler, http);
            }
            catch (Exception ex)
            {
                http.set_error_response(ex.Message);
                throw;
            }
            finally
            {
                notifyCompletion();
            }
        }

        private void invoke_async_handler(HttpContext http, SingleRoute route, Action notifyCompletion)
        {
            try
            {
                route.AsyncHandler(http, notifyCompletion);
            }
            catch (Exception ex)
            {
                http.set_error_response(ex.Message);
                notifyCompletion();
                throw;
            }
        }

        private void initialization_is_failed(string moduleName, Exception ex)
        {
            string msg = "Failed to initialize module '{0}'; {1}".Fmt(moduleName, ex.Message);

            _initializationFailureMsg = msg;
            TraceLogger.WriteLine(ex.FormatHierarchy(msg, true, true));
        }

        private void validate_initialization()
        {
            if (_initializationFailureMsg == null)
                return;

            throw 
                new WebApiException(_initializationFailureMsg);
        }

        #endregion

        #region interface

        SingleRoute IWebApiController.GetRoute(string path)
        {
            validate_initialization();

            var route = find_route(path);
            return route;
        }

        void IWebApiController.ProcessRequest(HttpContext http, SingleRoute route, Action notifyCompletion)
        {
            validate_initialization();

            if (route.HandlerIsSynchronous)
                invoke_handler(http, route, notifyCompletion);
            else
            if (route.HandlerIsAsynchronous)
                invoke_async_handler(http, route, notifyCompletion);
            else
                throw new WebApiException("No handler for route '{0}' is provided", route.Path);
        }

        void IWebApiController.InitializeModule(string name, ModuleFactory makeNewModule)
        {
            _log.Info("'{0}': initializing...", name);

            try
            {
                var module = makeNewModule(name, _log);
                _modules.Add(module);

                module.Initialize(this);

                _log.Info("'{0}': initialization is completed", name);
            }
            catch (Exception ex)
            {
                initialization_is_failed(name, ex);
            }
        }

        RequestConfig IWebApiController.GetRequestConfig(HttpContext http)
        {
            if (_makeRequestConfig == null)
                return null;

            var config = _makeRequestConfig(http);
            return config;
        }

        void IWebApiController.Shut()
        {
            var modules = _modules.ToArray();

            foreach (var module_ in modules)
            {
                var module = module_;

                Try.Catch.Handle(
                    () => module.Shut(),
                    ex => _log.Warn(ex, "Failed to shut module '{0}'", module.Name));
            }
        }

        SingleRoute IHttpServer.AddRoute(string path, IBlController controller, RouteHandler handler)
        {
            var route = add_route(new SingleRoute
                {
                    Path = path,
                    Controller = controller,
                    Handler = handler
                });

            return route;
        }

        SingleRoute IHttpServer.AddRoute(string path, IBlController controller, AsyncRouteHandler handler)
        {
            var route = add_route(new SingleRoute
                {
                    Path = path,
                    Controller = controller,
                    AsyncHandler = handler
                });

            return route;
        }

        void IHttpServer.ConfigureThrottling(IBlController listener, ThrottlingConfig config)
        {
        }

        void IHttpServer.SetWhiteIpList(IBlController listener, IpList list)
        {
        }

        #endregion
    }
}