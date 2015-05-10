using System;

namespace L4p.Common.Extensions
{
    public static class GuidExtensions
    {
        public static string AsStr(this Guid guid)
        {
            return
                guid.ToString("N").ToUpperInvariant();
        }

        public static string AsOracleStr(this Guid guid)
        {
            if (guid == Guid.Empty)
                return null;

            return
                guid.ToString("N").ToUpperInvariant();
        }

        public static bool IsEmpty(this Guid guid)
        {
            return
                guid == Guid.Empty;
        }

        public static string AsStr(this Guid? guid)
        {
            if (guid == null)
                return null;

            return
                guid.Value.ToString("N").ToUpperInvariant();
        }

        public static string AsOracleStr(this Guid? guid)
        {
            if (guid == null)
                return null;

            var value = guid.Value;

            if (value ==  Guid.Empty)
                return null;

            return
                value.ToString("N").ToUpperInvariant();
        }

        public static bool IsEmpty(this Guid? guid)
        {
            return
                guid == null ||
                guid.Value == Guid.Empty;
        }
    }
}