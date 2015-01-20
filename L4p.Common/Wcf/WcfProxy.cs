using System;
using System.ServiceModel;
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
            {}
        }

        #endregion

        #region contstruction

        protected WcfProxy(string uri)
            : base(WcfTcp.NewTcpBinding(), new EndpointAddress(uri))
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