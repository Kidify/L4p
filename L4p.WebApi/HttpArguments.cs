using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using L4p.Common;
using L4p.Common.Extensions;
using L4p.Common.Helpers;

namespace L4p.WebApi
{
    public interface IHttpArguments
    {
        HttpArgument this[params string[] names] { get; }
        bool Has(params string[] names);
        string DataAsJson();
    }

    public class HttpArgument
    {
        private readonly string _name;
        private readonly string _value;

        public HttpArgument(string name, string value)
        {
            _name = name;
            _value = value;
        }

        private T convert_value<T>()
        {
            Type type = typeof(T);

            try
            {
                return
                    _value.ConvertTo<T>();
            }
            catch (Exception ex)
            {
                throw new BadRequestException(ex,
                    "Failed to convert '{0}' value of '{1}' argument to '{2}' type", _value, _name, type.Name);
            }
        }

        public T As<T>()
        {
            if (_value == null)
                throw new BadRequestException("Required argument(s) '{0}' is not found in request", _name);

            return
                convert_value<T>();
        }

        public T As<T>(T defaultValue)
        {
            if (_value == null)
                return defaultValue;

            return
                convert_value<T>();
        }

        public T Populate<T>(T props)
        {
            return
                default(T);
        }
    }

    public class HttpArguments : IHttpArguments
    {
        #region members

        private readonly Dictionary<string, string> _args;
        private readonly ASplit _split;

        #endregion

        #region construction

        public static IHttpArguments New(HttpContext http)
        {
            var httpPath = http.Request.Path;
            var queryString = http.Request.QueryString;

            return
                new HttpArguments(httpPath, queryString);
        }

        public static IHttpArguments New(string httpPath, NameValueCollection queryString)
        {
            return
                new HttpArguments(httpPath, queryString);
        }

        private HttpArguments(string httpPath, NameValueCollection queryString)
        {
            _args = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            _split = new ASplit(httpPath);
            var segments = _split.Segments;

            for (int indx = 0; indx < segments.Length; indx++)
            {
                var segment = segments[indx];

                var lastOne =
                    indx + 1 == segments.Length;

                var value =
                    lastOne ? null : segments[indx + 1];

                _args[segment] = value;
            }

            if (queryString == null)
                return;

            foreach (var key in queryString.AllKeys)
            {
                if (key == null)
                    continue;

                _args[key] = queryString[key];
            }
        }

        #endregion

        #region private

        private HttpArgument get_argument(params string[] names)
        {
            Validate.That(names.IsNotEmpty());

            foreach (var name in names)
            {
                string value;

                if (_args.TryGetValue(name, out value))
                    return new HttpArgument(name, value);
            }

            return
                new HttpArgument(String.Join(",", names), null);
        }

        private bool has_argument(params string[] names)
        {
            Validate.That(names.IsNotEmpty());

            foreach (var name in names)
            {
                string value;

                if (_args.TryGetValue(name, out value))
                    return true;
            }

            return false;
        }

        #endregion

        #region interface

        HttpArgument IHttpArguments.this[params string[] names]
        {
            get { return get_argument(names); }
        }

        bool IHttpArguments.Has(params string[] names)
        {
            return
                has_argument(names);
        }

        string IHttpArguments.DataAsJson()
        {
            var json = get_argument("data").As<string>();

            if (json.IsEmpty())
                throw new WebApiException("data parameter is empty");

            return json;
        }

        #endregion
    }
}