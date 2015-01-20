using NUnit.Framework;

namespace L4p.Common.Helpers.nunit
{
    [TestFixture]
    class Md5HelpersTests
    {
        [Test]
        public void SimpleUsage()
        {

            string md5 = Md5Helpers.calculate_md5(
                new {
                        From = "2013-01-01 00:00:00", 
                        To = "2013-02-01 00:00:00",
                        PageIndex = 1,
                        SecretKey = "l4procks"
                    });

            Assert.That(md5, Is.EqualTo("d08de8130513288247381e40aeba5259"));
        }
    }
}