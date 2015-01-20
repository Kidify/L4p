using System;
using System.Runtime.Serialization;

namespace L4p.Common.FunnelsModel.comm
{
    [DataContract]
    public class StoreInfo
    {
        [DataMember] public string FunnelId { get; set; }
        [DataMember] public Guid StoreId { get; set; }
        [DataMember] public Guid ShopId { get; set; }
        [DataMember] public string ShopUri { get; set; }
        [DataMember] public Guid AgentId { get; set; }
        [DataMember] public string AgentUri { get; set; }
    }
}