using System;
using L4p.Common.Loggers;

namespace L4p.WebApi
{
    public interface IBlcBox
    {
        void RegisterRoutes(IHttpServer server);
    }

    public interface IBlcBox<T> : IBlcBox
        where T : IBlController
    {
    }

    public class BlcBox<T> : IBlcBox<T>
        where T : IBlController
    {
        #region members

        private readonly ILogFile _log;
        private readonly string _controllerName;
        private readonly IBlController _controller;

        #endregion

        #region construction

        public static IBlcBox<T> New(ILogFile log, Func<T> factoryMethod)
        {
            return
                new BlcBox<T>(log, factoryMethod);
        }

        private BlcBox(ILogFile log, Func<T> factoryMethod)
        {
            _log = log;
            _controllerName = typeof(T).Name;

            try
            {
                _controller = factoryMethod();
                _log.Info("Controller '{0}' is initialized", _controllerName);
            }
            catch (Exception ex)
            {
                log.Error(ex, "Failed to instantiate controller '{0}'", _controllerName);
            }
        }

        #endregion

        #region private
        #endregion

        #region interface

        void IBlcBox.RegisterRoutes(IHttpServer server)
        {
            if (_controller == null)
            {
                _log.Warn("Skipping route registration of controller '{0}' since its instantiation has failed", _controllerName);
                return;
            }

            try
            {
                _controller.RegisterRoutes(server);
                _log.Info("Routes of controller '{0}' are registered", _controllerName);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Failed to register routes of controller '{0}'", _controllerName);
            }
        }

        #endregion
    }
}