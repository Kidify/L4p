using System;

namespace L4p.Common.MsgIsApplications
{
    class MsgValueBox
    {
        public Type Type { get; set; }
        public object Value { get; set; }
        public bool PartOfSerializedContext { get; set; }
        public string Json { get; set; }
    }
}