using System.IO;
using System.Web;
using NUnit.Framework;

namespace L4p.WebApi._nunit
{
    [TestFixture]
    class HttpHelpersTests
    {
        [Test]
        public void get_domain_tests()
        {
            Assert.That(HttpHelpers.get_base_domain("l4p.com"), Is.EqualTo("l4p.com"));
            Assert.That(HttpHelpers.get_base_domain("api.l4p.com"), Is.EqualTo("l4p.com"));
            Assert.That(HttpHelpers.get_base_domain("api.dev.l4p.com"), Is.EqualTo("l4p.com"));
        }

        [Test]
        public void get_http_item_When_no_item_is_there()
        {
            var request = new HttpRequest("", "http://a.b.com", "");
            var response = new HttpResponse(new StringWriter());
            var http = new HttpContext(request, response);

            var item = http.get_item<HttpHelpersTests>();
            Assert.Null(item);
        }
    }
}