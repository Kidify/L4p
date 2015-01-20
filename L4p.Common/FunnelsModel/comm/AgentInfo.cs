using System;
using System.Runtime.Serialization;

namespace L4p.Common.FunnelsModel.comm
{
    [DataContract]
    public class AgentInfo
    {
        [DataMember] public Guid AgentId { get; set; }
        [DataMember] public string Uri { get; set; }
    }
}