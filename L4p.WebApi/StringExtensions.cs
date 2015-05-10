using System;
using System.Web;
using L4p.Common.Extensions;

namespace L4p.WebApi
{
    public static class StringExtensions
    {
        public static string UrlEncode(this string str)
        {
            if (str.IsEmpty())
                return str;

            return
                HttpUtility.UrlEncode(str);
        }
    }
}