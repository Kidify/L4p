using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using L4p.Common.Loggers;

namespace L4p.Common.Wcf
{
    public interface IWcfProxy : IDisposable
    {
        void Close();
    }

    public class WcfProxy<T> : ClientBase<T>, IWcfProxy
        where T : class
    {
        #region private

        private void close_proxy()
        {
            try
            {
                Close();
            }
            catch
            {
                Abort();
            }
        }

        #endregion

        #region contstruction

        protected WcfProxy(string uri, Binding binding = null)
            : base(binding ?? WcfTcp.NewTcpBinding(), new EndpointAddress(uri))
        {
        }

        #endregion

        #region interface

        void IDisposable.Dispose()
        {
            close_proxy();
        }

        void IWcfProxy.Close()
        {
            close_proxy();
        }

        #endregion
    }
}