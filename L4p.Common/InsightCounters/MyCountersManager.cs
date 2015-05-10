namespace L4p.Common.InsightCounters
{
    public interface IMyCountersManager
    {
        T New<T>(object holder) where T : class, new();
    }

    class MyCountersManager : IMyCountersManager
    {
        #region members
        #endregion

        #region construction

        public static IMyCountersManager New()
        {
            return
                new MyCountersManager();
        }

        private MyCountersManager()
        {
        }

        #endregion

        #region private
        #endregion

        #region interface

        T IMyCountersManager.New<T>(object holder)
        {
            return
                new T();
        }

        #endregion
    }
}