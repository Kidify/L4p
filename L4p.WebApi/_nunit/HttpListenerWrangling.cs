using System.IO;
using System.Net;
using System.Threading;
using L4p.Common.Extensions;
using L4p.Common.Json;
using NUnit.Framework;
using NUnit.Framework.Compatibility;

namespace L4p.WebApi._nunit
{
    [TestFixture]
    class HttpListenerWrangling
    {
        [Test]
        public void FireOne()
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://+:8080/");
            listener.Start();

            var ctx = listener.GetContext();
            var request = ctx.Request;
            var response = ctx.Response;

            var writer = new StreamWriter(response.OutputStream);
            writer.WriteLine("A,B,C,D\t10,20,30,40");
            writer.Flush();
            response.Close();

            listener.Stop();
        }

        [Test]
        public void FireMany()
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://+:8080/");
            listener.Prefixes.Add("http://+:8082/");
            listener.Start();

            while (true)
            {
                var ctx = listener.GetContext();
                var request = ctx.Request;
                var response = ctx.Response;
                var writer = new StreamWriter(response.OutputStream);

                var dump = new {
                    request.HttpMethod,
                    request.IsLocal,
//                    request.LocalEndPoint,
                    request.ProtocolVersion,
                    request.QueryString,
                    request.RawUrl,
//                    request.RemoteEndPoint,
                    request.ServiceName,
                    request.Url,
                    request.UserAgent,
                    request.UserHostAddress,
                    request.UserHostName
                };

//                writer.WriteLine(dump.ToJson());
//                writer.WriteLine(request.Url.ToJson());
                writer.WriteLine("A,B,C,D 1,2,3,4");
                writer.Flush();
                response.Close();
            }

//            listener.Stop();
        }
    }
}
