using System;
using System.Dynamic;
using System.IO;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using L4p.Common.Extensions;
using L4p.Common.Helpers;
using L4p.Common.Json;

namespace L4p.WebApi
{
    public static class HttpHelpers
    {
        public static void set_item<T>(this HttpContext http, object request)
        {
            var type = typeof(T);
            http.Items[type] = request;
        }

        public static T get_item<T>(this HttpContext http)
            where T : class
        {
            var type = typeof(T);

            var item = (T) http.Items[type];

            if (item == null)
                throw new WebApiException("Item of type '{0}' is missing in http.Items", type.Name);

            return item;
        }

        public static T get_item<T>(this HttpContext http, T @default)
            where T : class
        {
            var type = typeof(T);

            var item = (T) http.Items[type] ?? @default;

            return item;
        }

        public static void set_cors(this HttpContext http, string origin)
        {
            http.Response.Headers.Set("Access-Control-Allow-Origin", origin);
        }

        public static void set_text_response(this HttpContext http, string text)
        {
            http.Response.ContentType = "text/plain";
            http.Response.Write(text);
        }

        public static void set_html_response(this HttpContext http, string text)
        {
            http.Response.ContentType = "text/html";
            http.Response.Write(text);
        }

        public static void set_json_response(this HttpContext http, string text)
        {
            http.Response.ContentType = "application/json";
            http.Response.Write(text);
        }

        private static readonly JsonConverter _dateTimeConverter = new IsoDateTimeConverter { DateTimeFormat = "dd-MMM-yyyy HH:mm:ss" };
        private static readonly JsonConverter _enumConverter = new StringEnumConverter();

        public static void set_json_response<T>(this HttpContext http, T data)
        {
            string json = JsonConvert.SerializeObject(data, _enumConverter, _dateTimeConverter);

            http.Response.ContentType = "application/json";
            http.Response.Write(json);
        }

        public static void set_json_response<T>(this HttpContext http, int statusCode, T data)
        {
            string json = JsonConvert.SerializeObject(data, _enumConverter, _dateTimeConverter);

            http.Response.StatusCode = statusCode;
            http.Response.ContentType = "application/json";
            http.Response.Write(json);
        }

        public static void set_jsonp_response<T>(this HttpContext http, T data, IHttpArguments args = null)
        {
            if (args == null)
                args = HttpArguments.New(http);

            var callback = args["callback"].As<string>("");

            if (callback.IsEmpty())
            {
                http.set_json_response(data);
                return;
            }

            string json = JsonConvert.SerializeObject(data, _enumConverter, _dateTimeConverter);
            string text = "{0}({1})".Fmt(callback, json);

            http.Response.ContentType = "text/javascript";
            http.Response.Write(text);
        }

        public static void set_error_response(this HttpContext http, int statusCode, string text)
        {
            http.Response.StatusCode = statusCode;
            http.Response.ContentType = "text/plain";
            http.Response.Write(text);
        }

        public static void set_error_response(this HttpContext http, string errMsg)
        {
            var reply = new ErrorReply {Error = errMsg};

            http.Response.ContentType = "application/json";
            http.Response.Write(reply.AsSingleLineJson());
        }

        public static void set_xml_response(this HttpContext http, string text)
        {
            http.Response.ContentType = "text/xml";
            http.Response.Write(text);
        }

        public static void set_png_responce(this HttpContext http, Stream image)
        {
            http.Response.ContentType = "image/png";
            image.CopyTo(http.Response.OutputStream);
        }

        public static string input_stream_2_text(this HttpContext http)
        {
            using (var reader = new StreamReader(http.Request.InputStream))
            {
                string text = reader.ReadToEnd();
                return text;
            }
        }

        public static string uploaded_file_2_string(this HttpContext http)
        {
            var file = http.Request.Files.Get(0);
            var reader = new StreamReader(file.InputStream);
            string data = reader.ReadToEnd();

            return data;
        }

        public static void validate_host_ip(this HttpContext http, params string[][] whiteLists)
        {
            if (whiteLists == null)
                return;

            string forwardedIps;
            string ip = http.get_client_ip(out forwardedIps);

            foreach (var whiteList in whiteLists)
            {
                if (whiteList == null)
                    continue;

                bool isThere =
                    Array.Find(whiteList, x => x == ip) != null;

                if (isThere)
                    return;
            }

            throw
                new BadRequestException("Access is denied; ip='{0}'; forwarded ips '{1}'", ip, forwardedIps);
        }

        public static HttpContext make_http_context(this string path, string data = null)
        {
            var url = "http://a.b.c.com/{0}".Fmt(path);
            var queryStr = "data={0}".Fmt(data ?? "");

            var request = new HttpRequest("", url, queryStr);
            var response = new HttpResponse(new StringWriter());
            var http = new HttpContext(request, response);

            return http;
        }

        public static dynamic call_handler(Action<HttpContext> handler, object data)
        {
            var dataAsStr = 
                JsonConvert.SerializeObject(data)
                .Replace('"', '\'');

            var url = "http://a.b.c.com/stub";
            var queryStr = "data={0}".Fmt(dataAsStr);

            var request = new HttpRequest("", url, queryStr);
            var response = new HttpResponse(new StringWriter());
            var http = new HttpContext(request, response);

            handler(http);

            var replyAsStr = response.Output.ToString();
            var reply = JsonConvert.DeserializeObject<ExpandoObject>(replyAsStr, new ExpandoObjectConverter());

            return reply;
        }

        static internal string get_base_domain(string host)
        {
            var startFrom = host.Length;
            var dotNo = 0;

            while (--startFrom > 0)
            {
                if (host[startFrom] != '.')
                    continue;

                dotNo++;

                if (dotNo == 2)
                    break;
            }

            if (host[startFrom] == '.')
                startFrom++;

            var domain = host.Substring(startFrom);

            return domain;
        }

        public static string get_base_domain(this HttpContext http)
        {
            return
                get_base_domain(http.Request.Url.Host);
        }

        public static bool it_is_my_domain(this HttpContext http, string url)
        {
            var uri = new Uri(url, UriKind.Absolute);

            var myDomain = get_base_domain(http.Request.Url.Host);
            var otherDomain = get_base_domain(uri.Host);

            return
                0 == String.Compare(myDomain, otherDomain, StringComparison.InvariantCultureIgnoreCase);
        }

        public static void redirect(this HttpContext http, string redirectTo)
        {
            http.Response.Redirect(redirectTo, false);
        }

        public static string get_client_ip(this HttpContext http, out string forwardedIps)
        {
            var request = http.Request;

            forwardedIps = request.Headers.Get("X-Forwarded-For");
            var mostCloseIp = request.UserHostAddress;

            if (forwardedIps.IsEmpty())
                return mostCloseIp;

            var list = forwardedIps.Split(',');

            foreach (var ip in list)
            {
                if (ip.Split('.').Length != 4)
                    continue;

                return ip;
            }

            return mostCloseIp;
        }

        public static string get_client_ip(this HttpContext http)
        {
            string tmp;

            return
                http.get_client_ip(out tmp);
        }
    }
}