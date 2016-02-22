using System;
using System.Diagnostics;
using System.ServiceModel;
using L4p.Common.Extensions;
using L4p.Common.Loggers;

namespace L4p.Common.Wcf
{
    public interface IWcfHost : IDisposable
    {
        void StartAt(string uri);
        void Close();
    }

    public interface IWcfHost<T> : IWcfHost
        where T : class
    {
        T Target { get; set; }
    }

    public class WcfHost<T> : IWcfHost<T>
        where T : class
    {
        #region members

        private readonly string _serviceName;
        private readonly ILogFile _log;

        private T _target;
        private ServiceHost _host;

        #endregion

        #region construction

        public static IWcfHost<T> New(ILogFile log, T target = null)
        {
            return
                new WcfHost<T>(target, log.WrapIfNull());
        }

        public static IWcfHost<T> NewAsync(ILogFile log, T target = null)
        {
            return
                WcfAsyncHost<T>.New(log,
                new WcfHost<T>(target, log.WrapIfNull()));
        }

        private WcfHost(T target, ILogFile log)
        {
            _target = target;
            _log = log;

            _serviceName = typeof(T).AsServiceName();
        }
        
        #endregion

        #region private

        private static string get_host_uri(ServiceHost host)
        {
            var epoints = host.Description.Endpoints;

            if (epoints.Count == 0)
                return "unknown";

            var epoint = epoints[0];

            return
                epoint.ListenUri.ToString();
        }

        private void close_wcf_host(ServiceHost host)
        {
            if (host == null)
                return;

            var uri = get_host_uri(host);

            try
            {
                host.Close();
                _log.Info("service '{0}' is stopped at '{1}'", _serviceName, uri);
            }
            catch (Exception ex)
            {
                host.Abort();
                _log.Warn("Failed to stop service '{0} at '{1}'; {2}", _serviceName, uri, ex.Message);
            }
        }

        private void close_wcf_peer()
        {
            var host = _host;
            _host = null;

            close_wcf_host(host);
        }

        private ServiceHost open_wcf_peer(T target, string uri)
        {
            _log.Trace("starting service '{0}' at '{1}'", _serviceName, uri);

            var host = new ServiceHost(target);

            host.AddServiceEndpoint(
                implementedContract: typeof(T),
                binding: WcfTcp.NewTcpBinding(),
                address: uri);

            try
            {
                host.Open();
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Failed to open wcf peer at '{0}'", uri);
                throw;
            }

            _log.Info("service '{0}' is started at '{1}'", _serviceName, uri);

            return host;
        }

        #endregion

        #region interface

        void IDisposable.Dispose()
        {
            close_wcf_peer();
        }

        void IWcfHost.StartAt(string uri)
        {
            if (_target == null)
                throw new L4pException("Target has not been set for '{0}'", uri);

            if (_host != null)
                close_wcf_peer();

            NetTcpSharing.Instance.Enable();

            _host = open_wcf_peer(_target, uri);
        }

        void IWcfHost.Close()
        {
            close_wcf_peer();
        }

        T IWcfHost<T>.Target
        {
            get { return _target; }
            set { _target = value; }
        }

        #endregion
    }
}