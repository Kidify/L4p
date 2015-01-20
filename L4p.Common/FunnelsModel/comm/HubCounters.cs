using System.Runtime.Serialization;

namespace L4p.Common.FunnelsModel.comm
{
    [DataContract]
    public class HubCounters
    {
        [DataMember] public int ShopIsConnected { get; set; }
    }
}