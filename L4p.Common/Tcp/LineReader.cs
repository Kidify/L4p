using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using L4p.Common.Extensions;
using L4p.Common.Helpers;
using L4p.Common.Loggers;

namespace L4p.Common.Tcp
{
    public interface ILineReader
    {
        string ReadLine(TimeSpan timeout);
        void Reconnect();
        void Close();

        event Action<string> Connected;
        event Action<string> Disconnected;
        event Action<string> TryingToConnect;
        event Action<string> FailedToConnect;
        event Action<string> FailedToRead;
        event Action<string> NotEmptyLineIsReady;
    }

    class LineReader : ILineReader
    {
        #region members

        private readonly ILogFile _log;
        private readonly string _host;
        private readonly int _port;
        private readonly string _info;

        private event Action<string> _connected;
        private event Action<string> _disconnected;
        private event Action<string> _tryingToConnect;
        private event Action<string> _failedToConnect;
        private event Action<string> _failedToRead;
        private event Action<string> _notEmptyLineIsReady;

        private TcpClient _client;
        private StreamReader _tcp;

        #endregion

        #region construction

        public static ILineReader New(string url, ILogFile log, TimeSpan? receiveTimeout = null)
        {
            string host = null;
            int port = 0;

            Try.Catch.Wrap(
                () => TcpHelpers.ParseHostAndPort(url, out host, out port),
                ex => new TcpException(ex, "Failed to parse url '{0}'", url));

            if (host.IsEmpty())
                throw new TcpException("Failed to get host from '{0}' url", url);

            if (port == 0)
                throw new TcpException("Failed to get port from '{0}' url", url);

            return
                new LineReader(host, port, log, receiveTimeout);
        }

        public static ILineReader New(string host, int port, ILogFile log, TimeSpan? receiveTimeout = null)
        {
            return
                new LineReader(host, port, log, receiveTimeout);
        }

        private LineReader(string host, int port, ILogFile log, TimeSpan? receiveTimeout)
        {
            _log = log;
            _host = host;
            _port = port;
            _info = "'{0}:{1}'".Fmt(_host, _port);
        }

        #endregion

        #region private

        private void raise_event(Action<string> @event, string fmt, params object[] args)
        {
            if (@event == null)
                return;

            try
            {
                string msg = fmt.Fmt(args);
                @event(msg);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        private void ensure_connection()
        {
            if (_tcp != null)
                return;

            raise_event(_tryingToConnect, "connecting to {0}", _info);

            var client = new TcpClient(_host, _port);
            var tcp = new StreamReader(client.GetStream());

            client.ReceiveTimeout = 5000;

            raise_event(_connected, "{0} is connected", _info);

            _client = client;
            _tcp = tcp;

            // skip first line as it may be incomplete

            read_line();
        }

        private void close_connection()
        {
            if (_tcp == null)
                return;

            var client = _client;
            var tcp = _tcp;

            _client = null;
            _tcp = null;

            try
            {
                tcp.Close();
                client.Close();
            }
            catch
            {}
        }

        private string read_line()
        {
            string line = Try.Catch.Rethrow(
                () => _tcp.ReadLine(),
                ex => _log.Info("Tcp client has disconnected '{0}'", _info));

            return line;
        }

        private string read_not_empty_line()
        {
            while (true)
            {
                string line = read_line();

                if (line == null)
                {
                    _log.Info("Tcp client end-of-stream '{0}'", _info);
                    throw new TcpException(
                        "End of stream on {0} is reached", _info);
                }

                if (String.IsNullOrWhiteSpace(line))
                    continue;

                return line;
            }
        }

        private void eliminate_warnings()
        {
            raise_event(_disconnected, "");
            raise_event(_notEmptyLineIsReady, "");
        }

        #endregion

        #region ILineReader

        string ILineReader.ReadLine(TimeSpan timeout)
        {
            var tm = Stopwatch.StartNew();

            try
            {
                ensure_connection();
            }
            catch (Exception ex)
            {
                raise_event(_failedToConnect, "connection to {0} has failed; {1}", _info, ex.Message);
                return null;
            }

            try
            {
                return
                    read_not_empty_line();
            }
            catch (Exception ex)
            {
                raise_event(_failedToRead, "failed to read from {0}; {1}", _info, ex.Message);
                close_connection();
                return null;
            }
        }

        void ILineReader.Reconnect()
        {
            close_connection();
        }

        void ILineReader.Close()
        {
            close_connection();
        }

        event Action<string> ILineReader.Connected
        {
            add { _connected += value; }
            remove { _connected -= value; }
        }

        event Action<string> ILineReader.Disconnected
        {
            add { _disconnected += value; }
            remove { _disconnected -= value; }
        }

        event Action<string> ILineReader.TryingToConnect
        {
            add { _tryingToConnect += value; }
            remove { _tryingToConnect -= value; }
        }

        event Action<string> ILineReader.FailedToConnect
        {
            add { _failedToConnect += value; }
            remove { _failedToConnect -= value; }
        }

        event Action<string> ILineReader.FailedToRead
        {
            add { _failedToRead += value; }
            remove { _failedToRead -= value; }
        }

        event Action<string> ILineReader.NotEmptyLineIsReady
        {
            add { _notEmptyLineIsReady += value; }
            remove { _notEmptyLineIsReady -= value; }
        }

        #endregion
    }
}