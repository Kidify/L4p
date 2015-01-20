using System;
using System.Runtime.Serialization;
using L4p.Common.FunnelsModel.io;

namespace L4p.Common.FunnelsModel.comm
{
    [DataContract]
    public class Post
    {
        [DataMember] public string FunnelId { get; set; }
        [DataMember] public string Tag { get; set; }
        [DataMember] public string Path { get; set; }
        [DataMember] public string Json { get; set; }

        internal Ioop Ioop { get; set; }
    }
}