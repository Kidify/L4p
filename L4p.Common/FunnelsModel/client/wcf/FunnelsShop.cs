using System;
using L4p.Common.FunnelsModel.comm;
using L4p.Common.FunnelsModel.io;
using L4p.Common.Wcf;

namespace L4p.Common.FunnelsModel.client.wcf
{
    class FunnelsShop 
        : WcfProxy<comm.IFunnelsShop>, comm.IFunnelsShop
    {
        public static comm.IFunnelsShop New(string uri) { return new FunnelsShop(uri); }
        private FunnelsShop(string uri) : base(uri) {}

        ShopInfo comm.IFunnelsShop.GetInfo() { return Channel.GetInfo(); }
        ShopInfo comm.IFunnelsShop.RegisterStore(StoreInfo info) { return Channel.RegisterStore(info); }
        void comm.IFunnelsShop.StoreIsRemoved(Guid storeId) { Channel.StoreIsRemoved(storeId); }
        void comm.IFunnelsShop.PublishPost(Guid storeId, Post post) { Channel.PublishPost(storeId, post); }
        Post comm.IFunnelsShop.GetPost(Guid storeId, string path) { return Channel.GetPost(storeId, path); }
        void comm.IFunnelsShop.KeepAlive() { Channel.KeepAlive(); }
    }

    class FunnelsShopAsyncWriter
        : WcfProxy<comm.IFunnelsShopAsyncWriter>, comm.IFunnelsShop
    {
        public static comm.IFunnelsShop New(string uri) { return new FunnelsShopAsyncWriter(uri); }
        private FunnelsShopAsyncWriter(string uri) : base(uri) { }

        #region private

        private void complete_io(Ioop ioop, Action completionCallback)
        {
            try
            {
                completionCallback();
            }
            catch (Exception ex)
            {
                ioop.OnIoError(ex);
            }

            ioop.OnIoComplete();
        }

        #endregion

        ShopInfo comm.IFunnelsShop.GetInfo() { return Channel.GetInfo(); }
        ShopInfo comm.IFunnelsShop.RegisterStore(StoreInfo info) { return Channel.RegisterStore(info); }
        void comm.IFunnelsShop.StoreIsRemoved(Guid storeId) { Channel.StoreIsRemoved(storeId); }
        Post comm.IFunnelsShop.GetPost(Guid storeId, string path) { return Channel.GetPost(storeId, path); }
        void comm.IFunnelsShop.KeepAlive() { Channel.KeepAlive(); }

        void comm.IFunnelsShop.PublishPost(Guid storeId, Post post)
        {
            var ioop = post.Ioop;

            AsyncCallback callback = ar => 
                complete_io(ioop, () => Channel.EndPublishPost(ar));

            try
            {
                Channel.BeginPublishPost(storeId, post, callback, null);
            }
            catch (Exception ex)
            {
                ioop.OnIoError(ex);
                throw;
            }
        }
    }
}