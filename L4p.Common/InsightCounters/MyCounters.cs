namespace L4p.Common.InsightCounters
{
    public class MyCounters
    {
        #region members

        public static IMyCountersManager _instance;

        #endregion 

        #region singleton

        static MyCounters()
        {
            _instance = MyCountersManager.New();
        }

        public static IMyCountersManager Instance
        {
            get { return _instance; }
            set { _instance = value; }
        }

        #endregion

        #region interface

        public static T New<T>(object holder) where T : class, new()
        {
            return
                _instance.New<T>(holder);
        }

        public static void Clear<T>(T counters) where T : class, new()
        {
        }

        #endregion
    }
}