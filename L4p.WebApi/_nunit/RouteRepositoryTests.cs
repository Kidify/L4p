using System;
using System.Web;
using NUnit.Framework;

namespace L4p.WebApi._nunit
{
    [TestFixture]
    class RouteRepositoryTests
    {
        [Test]
        public void RegisterRoutes_tests()
        {
            var repo = RouteRepository.New();

            var r1 = new SingleRoute {Path = "/reports/bfp/data.xml"};
            var r2 = new SingleRoute {Path = "/repORts/bfp/data2.xml"};

            var r3 = new SingleRoute {Path = "/reports/bfp/test/data.xml"};
            var r4 = new SingleRoute {Path = "/rePorts/bfp/tesT/daTa2.xml"};

            repo.AddRoute(r1);
            repo.AddRoute(r2);
            repo.AddRoute(r3);
            repo.AddRoute(r4);

            Assert.That(repo.FindRoute("/reports/bfp/data.xml"), Is.SameAs(r1));
            Assert.That(repo.FindRoute("/reports/bfp/data2.xml"), Is.SameAs(r2));

            Assert.That(repo.FindRoute("/reports/bfp/dbg/data.xml"), Is.SameAs(r1));
            Assert.That(repo.FindRoute("/reports/bfp/dbg/data2.xml"), Is.SameAs(r2));

            Assert.That(repo.FindRoute("/reports/bfp/test/data.xml"), Is.SameAs(r3));
            Assert.That(repo.FindRoute("/reports/bfp/test/data2.xml"), Is.SameAs(r4));

            Assert.That(repo.FindRoute("/reports/test/bfp/data.xml"), Is.SameAs(r3));
            Assert.That(repo.FindRoute("/reports/test/bfp/data2.xml"), Is.SameAs(r4));

            Assert.That(repo.FindRoute("/reports/bfp/tEst/dbg/data.xml"), Is.SameAs(r3));
            Assert.That(repo.FindRoute("/reports/bfp/dbg/test/data2.xml"), Is.SameAs(r4));

            Assert.That(repo.FindRoute("/reports/bfp/id/{id}/DATA.XML"), Is.SameAs(r1));
            Assert.That(repo.FindRoute("/reports/BFP/id/{id}/test/data.xml"), Is.SameAs(r3));
        }

        [Test]
        public void RegisterRoutes_not_found_tests()
        {
            var repo = RouteRepository.New();
            repo.AddRoute(new SingleRoute { Path = "/reports/affiliate/4665/data.xml" });

            Assert.That(repo.FindRoute("/reports"), Is.Null);
            Assert.That(repo.FindRoute("/reports/affiliate/1234/data.xml"), Is.Null);
            Assert.That(repo.FindRoute("/reports/affiliate/data.xml"), Is.Null);
        }

        [Test, Ignore("not implemented yet")]
        public void RegisterRoutes_optional_terms()
        {
            var repo = RouteRepository.New();
            var r1 = new SingleRoute {Path = "/reports/bfp/dbg/data.xml"};
        }

        [Test]
        public void RegisterRoutes_positional_vars()
        {
            var http = "/reports/affiliate/1234/data.xml".make_http_context();
            var args = HttpArguments.New(http);

            int affiliateId = args["affiliate"].As<int>();
            Assert.That(affiliateId, Is.EqualTo(1234));
        }
    }
}
