using NUnit.Framework;

namespace L4p.Common.Tcp.nunit
{
    [TestFixture]
    class When_uri_is_parsed
    {
        [Test]
        public void It_should_parse_host_and_port()
        {
            string host;
            int port;

            TcpHelpers.ParseHostAndPort("ams-quote03:1946", out host, out port);
            Assert.That(host, Is.EqualTo("ams-quote03"));
            Assert.That(port, Is.EqualTo(1946));
        }
    }
}