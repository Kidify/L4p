using System.ServiceModel;

namespace L4p.Common.Wcf
{
    public class TcpStreamBindng : NetTcpBinding
    {
        public TcpStreamBindng(int maxMsgSize)
        {
            TransferMode = TransferMode.Streamed;

            MaxReceivedMessageSize = maxMsgSize;
            MaxBufferSize = maxMsgSize;
            MaxBufferPoolSize = maxMsgSize;

            ReaderQuotas.MaxStringContentLength = maxMsgSize;
            ReaderQuotas.MaxArrayLength = maxMsgSize;

            Security.Mode = SecurityMode.None;
        }
    }
}