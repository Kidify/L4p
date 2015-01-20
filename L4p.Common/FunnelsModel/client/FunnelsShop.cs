using System;
using System.Collections.Generic;
using System.Threading;
using L4p.Common.ActiveObjects;
using L4p.Common.Extensions;
using L4p.Common.FunnelsModel.comm;
using L4p.Common.FunnelsModel.io;
using L4p.Common.IoCs;
using L4p.Common.Wcf;

namespace L4p.Common.FunnelsModel.client
{
    interface IFunnelsShop
    {
        void PublishPost(Guid storeId, Post post);
        Post GetPost(Guid storeId, string path);
        void StoreIsRemoved(Guid storeId);
        void CloseConnection();
    }

    class FunnelsShop : IFunnelsShop
    {
        #region members

        private readonly StoreInfo _info;
        private readonly IIoSink _sink;
        private readonly IIoQueue _que;
        private readonly IIoWriter _writer;
        private readonly IIoReader _reader;
        private readonly IIoConnector _io;

        #endregion

        #region construction

        public static IFunnelsShop New(StoreInfo info, IIoC ioc)
        {
            return
                new FunnelsShop(info, ioc);
        }

        private FunnelsShop(StoreInfo info, IIoC ioc)
        {
            _info = info;
            _sink = ioc.Resolve<IIoSink>();
            _que = ioc.Resolve<IIoQueue>();

            _io = ioc.Resolve<IIoConnector>();
            _writer = IoWriter.New(ioc, info);
            _reader = IoReader.New(ioc, info);
        }

        #endregion

        #region private

        private void init_ioop(Ioop ioop)
        {}

        private void set_services(Ioop ioop)
        {
            ioop.Services.Que = _que;
            ioop.Services.Out = _writer;
            ioop.Services.In = _reader;
        }

        #endregion

        #region interface

        void IFunnelsShop.PublishPost(Guid storeId, Post post)
        {
            var ioop = new Ioop();

            init_ioop(ioop);
            set_services(ioop);

            ioop.Io =
                shop => shop.PublishPost(storeId, post);

            post.Ioop = ioop;

            _sink.PostIo(ioop);
        }

        Post IFunnelsShop.GetPost(Guid storeId, string path)
        {
            var post = _reader.GetPost(storeId, path);
            return post;
        }

        void IFunnelsShop.StoreIsRemoved(Guid storeId)
        {
        }

        void IFunnelsShop.CloseConnection()
        {
        }

        #endregion
    }
}