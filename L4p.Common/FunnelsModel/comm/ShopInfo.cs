using System;
using System.Runtime.Serialization;

namespace L4p.Common.FunnelsModel.comm
{
    [DataContract]
    public class ShopInfo
    {
        [DataMember] public Guid ShopId { get; set; }
        [DataMember] public string Uri { get; set; }
    }
}