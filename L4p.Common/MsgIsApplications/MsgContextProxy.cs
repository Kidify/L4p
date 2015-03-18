using System.Dynamic;

namespace L4p.Common.MsgIsApplications
{
    public class MsgContextProxy : IMsgContext
    {
        #region members

        private readonly IMsgContext _impl;

        #endregion

        #region construction

        public MsgContextProxy()
        {
            _impl = MsgContext.NewSync();
        }

        public MsgContextProxy(IMsgContext impl)
        {
            _impl = impl;
        }

        #endregion

        #region private
        #endregion

        #region interface

        public IMsgContext Impl { get { return _impl.Impl; } }
        public IMsgContext Lock { set { _impl.Lock = value; } }

        public ExpandoObject Dump(dynamic root) { return _impl.Dump(root); }
        public T Add<T>(T value) where T : class, new() { return _impl.Add(value); }
        public T Get<T>() where T : class, new() { return _impl.Get<T>(); }
        public T Remove<T>() where T : class, new() { return _impl.Remove<T>(); }
        public T Replace<T>(T value) where T : class, new() { return _impl.Replace(value); }
        public void SetMsgIsTheApp(MsgIsTheAppEx msgIsTheApp) { _impl.SetMsgIsTheApp(msgIsTheApp); }

        #endregion
    }
}