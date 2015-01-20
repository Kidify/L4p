using System;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;

namespace L4p.Common.FunnelsModel.comm
{
    [ServiceContract]
    public interface IFunnelsShop
    {
        [OperationContract]
        ShopInfo GetInfo();

        [OperationContract]
        ShopInfo RegisterStore(StoreInfo info);

        [OperationContract]
        void StoreIsRemoved(Guid storeId);

        [OperationContract(Action="Funnels/PublishPost", ReplyAction = "Funnels/PublishPostReply")]
        [OneWay]
        void PublishPost(Guid storeId, Post post);

        [OperationContract] 
        Post GetPost(Guid storeId, string path);

        [OperationContract]
        [OneWay]
        void KeepAlive();
    }

    [ServiceContract]
    public interface IFunnelsShopAsyncWriter : IFunnelsShop
    {
        [OperationContract(Action="Funnels/PublishPost", ReplyAction = "Funnels/PublishPostReply", AsyncPattern = true)]
        [OneWay]
        IAsyncResult BeginPublishPost(Guid storeId, Post post, AsyncCallback callback, object state);
        void EndPublishPost(IAsyncResult ar);
    }
}