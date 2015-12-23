using System.Collections.Generic;

namespace L4p.Common.ConfigurationFiles
{
    class CraSection
    {
        public string ConfigKey { get; set; }
        public bool IsActive { get; set; }
        public string Environment { get; set; }
        public Dictionary<string, string> Configs { get; set; }
    }

    interface ICraRepo
    {
        CraSection GetSection(string configKey);
    }

    class CraRepo : ICraRepo
    {
        #region members

//        private readonly Dictionary<string, CraSection> _sections;

        #endregion

        #region construction

        public static ICraRepo New()
        {
            return
                new CraRepo();
        }

        private CraRepo()
        {
        }

        #endregion

        #region private
        #endregion

        #region interface

        CraSection ICraRepo.GetSection(string configKey)
        {
            return null;
        }

        #endregion
    }
}