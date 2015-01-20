using System;

namespace L4p.Common.FunnelsModel
{
    static class FunnelHelpers
    {
        public static string MakePath(string funnelId, string tag)
        {
            return
                String.Format("{0}/{1}", funnelId, tag);
        }
    }
}