namespace L4p.Common.MsgIsApplications
{
    public static class AnyMsgIsTheApp
    {
        #region members

        private static IMsgIsTheApp _instance;

        #endregion

        #region construction

        static AnyMsgIsTheApp()
        {
            _instance = MsgIsTheAppEx.New();
        }

        #endregion

        #region private
        #endregion

        #region interface

        public static IMsgIsTheApp Instance 
        {
            get { return _instance; }
            set { _instance = value; }
        }

        public static IMsgContext MsgIsTheApp(this object msg)
        {
            return
                _instance.GetContextFor(msg);
        }

        #endregion
    }
}