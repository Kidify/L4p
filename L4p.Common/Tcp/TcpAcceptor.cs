using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using L4p.Common.ForeverThreads;
using L4p.Common.Helpers;
using L4p.Common.Loggers;

namespace L4p.Common.Tcp
{
    public delegate void AcceptedCallback(string epoint, StreamWriter tcp);

    public interface ITcpAcceptor : IShouldBeStarted
    {
        void Start();
        void Stop();
    }

    public class TcpAcceptor : ITcpAcceptor
    {
        #region members

        private readonly ILogFile _log;
        private readonly int _port;
        private readonly AcceptedCallback _acceptedCallback;
        private readonly IForeverThread _thr;
        private readonly TcpListener _acceptor;

        #endregion

        #region construction

        public static ITcpAcceptor New(int port, ILogFile log, AcceptedCallback acceptedCallback)
        {
            return
                new TcpAcceptor(port, log.WrapIfNull(), acceptedCallback);
        }

        private TcpAcceptor(int port, ILogFile log, AcceptedCallback acceptedCallback)
        {
            _log = log;
            _port = port;
            _acceptedCallback = acceptedCallback;
            _thr = ForeverThread.New(accepting_loop, log);
            _acceptor = new TcpListener(IPAddress.Any, _port);
        }

        #endregion

        #region private

        private void accepting_loop()
        {
            while (true)
            {
                if (_thr.StopRequestIsPosted())
                    break;

                Thread.Sleep(1);        // busy wait since polling for connections

                bool noPendingConnections = !_acceptor.Pending();

                if (noPendingConnections)
                    continue;

                var client = _acceptor.AcceptTcpClient();
                var epoint = client.Client.RemoteEndPoint.ToString();
                _log.Info("got connection from '{0}'", epoint);

                var writer = new StreamWriter(client.GetStream());
                _acceptedCallback(epoint, writer);
            }
        }

        #endregion

        #region interface

        void ITcpAcceptor.Start()
        {
            var epoint = (IPEndPoint)_acceptor.LocalEndpoint;

            Try.Catch.Wrap(
                () => _acceptor.Start(),
                ex => new TcpException(ex, "Failed to listen to {0}", epoint));

            _log.Info("listening on port {0}", epoint.Port);

            _thr.Start(this.GetType().Name);
        }

        void ITcpAcceptor.Stop()
        {
            _thr.Stop();
        }

        #endregion
    }
}