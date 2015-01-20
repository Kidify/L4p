using System;
using L4p.Common.Helpers;
using L4p.Common.Json;

namespace L4p.Common.Extensions
{
    public static class StringExtensions
    {
        public static string Fmt(this string fmt, params object[] args)
        {
            string result = fmt;

            try
            {
                result = String.Format(fmt, args);
            }
            catch   // return the input pattern if failed to format
            { }

            return result;
        }

        public static string Substitute<T>(this string fmt, T values)
        {
            var result = fmt;
            var allIn = fmt;

            foreach (var property in values.GetType().GetProperties())
            {
                var value = property.GetValue(values, null);
                var name = property.Name;

                var prm = "{" + name + "}";

                result = result.Replace(prm, 
                    value != null ? value.ToString() : String.Empty);

                allIn = allIn.Replace(prm, "");
            }

            var indx = allIn.IndexOf('{');

            if (indx != -1)
                throw new L4pException("Not all parts are substituted; fmt='{0}' at index {1}; allIn='{2}'", fmt, indx, allIn);

            return result;
        }

        public static string Md5(this string str)
        {
            return
                Md5Helpers.calculate_md5(str);
        }

        public static bool IsEmpty(this string str)
        {
            return
                String.IsNullOrEmpty(str);
        }

        public static bool IsNotEmpty(this string str)
        {
            return
                !String.IsNullOrEmpty(str);
        }

        public static T Parse<T>(this string json)
        {
            return
                JsonHelpers.FromJson<T>(json);
        }

        public static object Parse(this string json, Type type)
        {
            return
                JsonHelpers.FromJson(json, type);
        }

        public static DateTime ToDateTime(this string date)
        {
            return
                DateTime.Parse(date);
        }

        public static TimeSpan ToTimeSpan(this string span)
        {
            return
                TimeSpan.Parse(span);
        }
    }
}