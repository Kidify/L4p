using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using L4p.Common.Extensions;
using L4p.Common.Helpers;

namespace L4p.Common.Tcp
{
    public static class TcpHelpers
    {
        public static void ParseHostAndPort(string url, out string host, out int port)
        {
            var parts = url.Split(':');

            host = parts[0];

            if (parts.Length > 1)
            {
                port = Try.Catch.Wrap(
                    () => int.Parse(parts[1]),
                    ex => new TcpException(ex, "Failed to parse port '{0}'", parts[1]));
            }
            else
            {
                port = 0;
            }
        }

        public static string GetMyIp()
        {
            var host = Dns.GetHostEntry(Environment.MachineName);

            var query =
                from ip in host.AddressList
                where ip.AddressFamily == AddressFamily.InterNetwork
                select ip.ToString();

            var myIp = query.FirstOrDefault();

            return myIp;
        }
    }
}