using System;

namespace L4p.Common.FunnelsModel.io
{
    class OpBehaviour
    { }

    class IoOperation
    {
        public Guid Id { get; set; }
        public int Index { get; set; }
        public DateTime At { get; set; }
        public OpBehaviour Behaviour { get; set; }
        public Guid StoreId { get; set; }
        public Action<comm.IFunnelsShop> Io { get; set; }
        public IoOperation Next { get; set; }
        public int ProxySeqNo { get; set; }
        public object IoState { get; set; }
    }

    class IoChain
    {
        public string ShopId { get; set; }
        public IoOperation FirstOp { get; set; }
        public IoOperation LastOp { get; set; }
        public IoOperation NextToStart { get; set; }
        public IoOperation NextToComplete { get; set; }
        public int ProxySeqNo { get; set; }
    }
}