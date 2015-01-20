using System;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;

namespace L4p.Common.FunnelsModel.comm
{
    [ServiceContract]
    interface IFunnelsAgent
    {
        [OperationContract] 
        [OneWay]
        void PruneFunnelData(Guid storeId, string path);

        [OperationContract]
        [OneWay]
        void ShopIsRemoved(ShopInfo shopInfo);
    }
}