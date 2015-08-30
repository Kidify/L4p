using System;
using System.Dynamic;
using System.IO;
using System.Net.Sockets;
using L4p.Common.DumpToLogs;
using L4p.Common.Extensions;
using L4p.Common.Helpers;
using L4p.Common.Loggers;

namespace L4p.Common.Tcp
{
    public interface ILineStreamReader : IDisposable, IHaveDump
    {
        string ReadLine();
        void CloseConnection();
    }

    public class LineStreamReader : ILineStreamReader
    {
        #region counters

        class Counters
        {
            public long Lines;
            public long EmptyLines;
        }

        #endregion

        #region members

        private readonly Counters _counters;
        private readonly ILogFile _log;
        private readonly string _host;
        private readonly int _port;
        private readonly TimeSpan? _receiveTimeout;
        private readonly string _info;

        private StreamReader _tcp;

        #endregion

        #region construction

        public static ILineStreamReader New(string url, ILogFile log, TimeSpan? receiveTimeout = null)
        {
            Validate.NotEmpty(url);

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
                new LineStreamReader(host, port, log, receiveTimeout);
        }

        public static ILineStreamReader New(string host, int port, ILogFile log, TimeSpan? receiveTimeout = null)
        {
            return
                new LineStreamReader(host, port, log, receiveTimeout);
        }

        private LineStreamReader(string host, int port, ILogFile log, TimeSpan? receiveTimeout)
        {
            _counters = new Counters();
            _log = log;
            _host = host;
            _port = port;
            _receiveTimeout = receiveTimeout;
            _info = "{0}:{1}".Fmt(_host, _port);
        }

        #endregion

        #region private

        private void ensure_connection()
        {
            if (_tcp != null)
                return;

            _log.Trace("connecting to '{0}' ...", _info);

            var client = new TcpClient(_host, _port);

            if (_receiveTimeout != null)
                client.ReceiveTimeout = (int) _receiveTimeout.Value.TotalMilliseconds;

            _tcp = new StreamReader(client.GetStream());

            // skip first line as it may be incomplete
            read_line();

            _log.Info("client is connected to '{0}'", _info);
        }

        private void close_connection()
        {
            if (_tcp == null)
                return;

            var tcp = _tcp;
            _tcp = null;

            try
            {
                tcp.Close();
            }
            catch
            {}
        }

        private string read_line()
        {
            string line = Try.Catch.Rethrow(
                () => _tcp.ReadLine(),
                ex => _log.Info("Tcp client is disconnected '{0}'; {1}", _info, ex.Message));

            _counters.Lines++;

            return line;
        }

        private string read_not_empty_line()
        {
            while (true)
            {
                string line = read_line();

                if (line == null)
                {
                    _log.Info("Tcp client end-of-stream is reached '{0}'", _info);
                    throw new TcpException(
                        "End of stream on '{0}' is reached", _info);
                }

                if (String.IsNullOrWhiteSpace(line))
                {
                    _counters.EmptyLines++;
                    continue;
                }

                return line;
            }
        }

        #endregion

        #region interface

        void IDisposable.Dispose()
        {
            close_connection();
        }

        ExpandoObject IHaveDump.Dump(dynamic root)
        {
            if (root == null)
                root = new ExpandoObject();

            root.Target = "{0}:{1}".Fmt(_host, _port);
            root.ReceiveTimeout = _receiveTimeout;
            root.Info = _info;
            root.Counters = _counters;

            return root;
        }

        string ILineStreamReader.ReadLine()
        {
            try
            {
                ensure_connection();
                return 
                    read_not_empty_line();
            }
            catch (Exception ex)
            {
                _log.Warn("Connection has failed '{0}' ({1})", _info, ex.Message);
                close_connection();
                throw;
            }
        }

        void ILineStreamReader.CloseConnection()
        {
            close_connection();
        }

        #endregion

    }
}