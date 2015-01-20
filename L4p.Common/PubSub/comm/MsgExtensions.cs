using System.Dynamic;
using System.Linq;
using L4p.Common.Json;

namespace L4p.Common.PubSub.comm
{
    static class MsgExtensions
    {
        private static object get_filters(this TopicFilterMsg msg)
        {
            if (msg.Filters == null)
                return null;

            var queryFilters =
                from filter in msg.Filters
                select new {filter.DefinedAt, filter.ContextAsJson};

            return
                queryFilters.ToArray();
        }

        public static string AsDscrStr(this TopicFilterMsg msg)
        {
            dynamic dscr = new ExpandoObject();

            dscr.TopicName = msg.TopicName;
            dscr.AgentToFilter = msg.AgentToFilter;
            dscr.SnapshotId = msg.SnapshotId;
            dscr.Filters = msg.get_filters();

            return 
                ((object) dscr).ToJson();
        }
    }
}