using System;
using System.Security.Cryptography;
using System.Text;

namespace L4p.Common.Helpers
{
    public static class Md5Helpers
    {
        public static string calculate_md5<T>(T data)
        {
            Type type = typeof(T);
            var sb = new StringBuilder();

            foreach (var prop in type.GetProperties())
            {
                object value = 
                    prop.GetValue(data, null) ?? "";

                sb.Append(value);
            }

            byte[] bytesToCheck = Encoding.UTF8.GetBytes(sb.ToString());
            byte[] hash = new MD5CryptoServiceProvider().ComputeHash(bytesToCheck);

            string md5 = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

            return md5;
        }

        public static string calculate_md5(string str)
        {
            byte[] bytesToCheck = Encoding.UTF8.GetBytes(str);
            byte[] hash = new MD5CryptoServiceProvider().ComputeHash(bytesToCheck);

            string md5 = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

            return md5;
        }
    }
}