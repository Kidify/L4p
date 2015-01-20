using System.Linq;

namespace L4p.WebApi
{
    public class IpList
    {
        public string[] List { get; set; }
    }

    static class IpListHelpers
    {
        public static bool ItIsMyIp(this IpList ips, string ip)
        {
            if (ips.List == null)
                return true;

            if (ips.List.Length == 0)
                return false;

            var here =
                ips.List.Any(x => x == ip);

            return here;
        }
    }
}