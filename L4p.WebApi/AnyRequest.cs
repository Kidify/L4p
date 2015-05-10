namespace L4p.WebApi
{
    public interface IAnyRequest
    {
        AnyRequest.IInArgs GetIn { get; }
        AnyRequest.IOutArgs GetOut { get; }
        AnyRequest.IMetaArgs GetMeta { get; }
    }

    public abstract class AnyRequest : IAnyRequest
    {
        private readonly MetaArgs _meta = new MetaArgs();

        public static readonly string Ok = "Ok";
        public static readonly string Failed = "Failed";

        public interface IInArgs
        {
            string TrackingId { get; set; }
        }

        public interface IOutArgs
        {
            string TrackingId { get; set; }
            string ErrorCode { get; set; }
            string ErrorMessage { get; set; }
        }

        public interface IMetaArgs
        {
            ELanguage Language { get; set; }
        }

        public class FailedArgs : IOutArgs
        {
            public string TrackingId { get; set; }
            public string ErrorCode { get; set; }
            public string ErrorMessage { get; set; }
        }

        class MetaArgs : IMetaArgs
        {
            public ELanguage Language { get; set; }
        }

        abstract protected IInArgs GetIn { get; }
        abstract protected IOutArgs GetOut { get; }
        virtual protected IMetaArgs GetMeta { get { return _meta; } }

        IInArgs IAnyRequest.GetIn { get { return GetIn; } }
        IOutArgs IAnyRequest.GetOut { get { return GetOut; } }
        IMetaArgs IAnyRequest.GetMeta { get { return GetMeta; } }
    }
}