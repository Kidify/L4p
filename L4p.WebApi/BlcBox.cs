using System;
using L4p.Common.Helpers;
using L4p.Common.Loggers;

namespace L4p.WebApi
{
    public interface IBlcBox
    {
        void Initialize(IHttpServer server);
        void Shut();
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

        void IBlcBox.Initialize(IHttpServer server)
        {
            if (_controller == null)
            {
                _log.Warn("Skipping route registration of controller '{0}' since its instantiation has failed", _controllerName);
                return;
            }

            try
            {
                _controller.Initialize(server);
                _log.Info("Routes of controller '{0}' are registered", _controllerName);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Failed to register routes of controller '{0}'", _controllerName);
            }
        }

        void IBlcBox.Shut()
        {
            if (_controller == null)
                return;

            Try.Catch.Handle(
                () => _controller.Shut(),
                ex => _log.Error(ex, "Failed to shut controller '{0}'", _controllerName));
        }

        #endregion
    }
}