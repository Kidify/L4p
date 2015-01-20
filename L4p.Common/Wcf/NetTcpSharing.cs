using System;
using System.ServiceProcess;
using L4p.Common.Loggers;

namespace L4p.Common.Wcf
{
    interface INetTcpSharing
    {
        void Enable();
    }

    class NetTcpSharing : INetTcpSharing
    {
        #region members

        private static readonly string NetTcpPortSharing = "NetTcpPortSharing";

        private readonly ILogFile _log;
        private bool _serviceIsRunning;

        #endregion

        #region singleton

        private static INetTcpSharing _instance = new NetTcpSharing();

        public static INetTcpSharing Instance
        {
            get { return _instance; }
        }

        #endregion

        #region construction

        private NetTcpSharing()
        {
            _log = LogFile.New("net-tcp-sharing.log");
        }

        #endregion

        #region private

        private bool is_automatic_service(ServiceController controller)
        {
            return
                controller.QueryStartMode() == ServiceStartMode.Automatic;
        }

        private void set_automatic_service(ServiceController controller)
        {
            controller.ChangeStartMode(ServiceStartMode.Automatic);
            _log.Info("service '{0}' is set to automatic mode", NetTcpPortSharing);
        }

        private bool is_service_running(ServiceController controller)
        {
            return
                controller.Status == ServiceControllerStatus.Running;
        }

        private void run_service(ServiceController controller)
        {
            controller.Start();
            _log.Info("service '{0}' is started", NetTcpPortSharing);
        }

        #endregion

        #region interface

        private void enable_and_run_service()
        {
            if (_serviceIsRunning)
                return;

            var controller = new ServiceController(NetTcpPortSharing);

            bool isAutomatic = is_automatic_service(controller);

            if (!isAutomatic)
                set_automatic_service(controller);

            bool isRunning = is_service_running(controller) ;

            if (!isRunning)
                run_service(controller);

            _serviceIsRunning = true;
            _log.Info("'{0}' should be running", NetTcpPortSharing);
        }

        void INetTcpSharing.Enable()
        {
            if (_serviceIsRunning)
                return;

            try
            {
                lock(this)
                    enable_and_run_service();
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Failure while enabling '{0}' service", NetTcpPortSharing);
            }
        }

        #endregion
    }
}