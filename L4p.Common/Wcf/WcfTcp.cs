using System.ServiceModel;

namespace L4p.Common.Wcf
{
    public class NetTcpSizedBindng : NetTcpBinding
    {
        public NetTcpSizedBindng(int maxMsgSize)
        {
            MaxReceivedMessageSize = maxMsgSize;
            MaxBufferSize = maxMsgSize;
            MaxBufferPoolSize = maxMsgSize;

            ReaderQuotas.MaxStringContentLength = maxMsgSize;
            ReaderQuotas.MaxArrayLength = maxMsgSize;

            Security.Mode = SecurityMode.None;
            PortSharingEnabled = true;
        }
    }

    public static class WcfTcp
    {
        private const int MAX_MSG_SIZE = 1*1024*1024;

        public static NetTcpBinding NewTcpBinding()
        {
            return
                new NetTcpSizedBindng(MAX_MSG_SIZE);
        }
    }
}