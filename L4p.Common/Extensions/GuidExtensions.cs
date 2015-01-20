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
            return
                guid.ToString("N").ToUpperInvariant();
        }

        public static bool IsEmpty(this Guid guid)
        {
            return
                guid == Guid.Empty;
        }
    }
}