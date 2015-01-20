using NUnit.Framework;
using L4p.Common.Loggers;

namespace L4p.Common.Tcp.nunit
{
    [TestFixture]
    class When_line_stream_reader_is_created
    {
        [Test]
        public void And_url_is_not_valid_It_throws()
        {
            Assert.That(() => LineStreamReader.New("ams-quote03:abcd", LogFile.Console), Throws.Exception);
        }
    }
}