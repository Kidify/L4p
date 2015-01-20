using System.ServiceModel;

namespace L4p.Common.FunnelsModel.comm
{
    [ServiceContract]
    public interface IFunnelsResolver
    {
        [OperationContract]
        void RegisterShop(ShopInfo info);

        [OperationContract]
        void ShopIsRemoved(ShopInfo info);

        [OperationContract]
        ShopInfo ResolveShop(StoreInfo info);
    }
}