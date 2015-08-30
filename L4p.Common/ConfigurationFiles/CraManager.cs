namespace L4p.Common.ConfigurationFiles
{
    interface ICraManager
    {
        bool IsUpToDate<TConfig>(string configKey);
        TConfig ReadConfig<TConfig>(string configKey);
    }

    class CraManager : ICraManager
    {
        #region members
        #endregion

        #region construction

        public static ICraManager New()
        {
            return
                new CraManager();
        }

        private CraManager()
        {
        }

        #endregion

        #region singleton

        private static readonly ICraManager _instance = new CraManager();

        public static ICraManager Instance
        {
            get { return _instance; }
        }


        #endregion

        #region private
        #endregion

        #region interface

        bool ICraManager.IsUpToDate<TConfig>(string configKey)
        {
            return false;
        }

        TConfig ICraManager.ReadConfig<TConfig>(string configKey)
        {
            return default(TConfig);
        }

        #endregion
    }
}