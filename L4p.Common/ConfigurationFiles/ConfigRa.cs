using System;
using L4p.Common.Loggers;

namespace L4p.Common.ConfigurationFiles
{
    public interface IConfigRa<TConfig>
        where TConfig : class, new()
    {
        TConfig Values { get; }
        event Action<TConfig> OnLoad;
    }

    public class ConfigRa<TConfig> : IConfigRa<TConfig>
        where TConfig : class, new()
    {
        #region members

        private readonly IConfigFile<TConfig> _configFile;

        private TConfig _config;
        private Action<TConfig> _onLoad;

        #endregion

        #region construction

        public static IConfigRa<TConfig> New(string path, ILogFile log = null)
        {
            log = log.WrapIfNull();

            return
                new ConfigRa<TConfig>(path, log);
        }

        public static IConfigRa<TConfig> New(TConfig config, ILogFile log = null)
        {
            log = log.WrapIfNull();

            return
                new ConfigRa<TConfig>(config, log);
        }

        protected ConfigRa(string path, ILogFile log)
        {
            _config = new TConfig();
            _configFile = ConfigFile<TConfig>.New(path, log);
        }

        protected ConfigRa(TConfig config, ILogFile log)
        {
            _config = config;
            _configFile = null;
        }

        #endregion

        #region private 
        
        void raise_on_load_event()
        {
            var onLoad = _onLoad;

            if (onLoad == null)
                return;

            var config = _config;
            onLoad(config);
        }

        private TConfig get_values()
        {
            if (_configFile == null)
                return _config;

            if (_configFile.IsUpToDate())
                return _config;

            _config = _configFile.Read();
            raise_on_load_event();

            return _config;
        }

        #endregion

        #region interface

        TConfig IConfigRa<TConfig>.Values
        {
            get { return get_values(); }
        }

        event Action<TConfig> IConfigRa<TConfig>.OnLoad
        {
            add { _onLoad += value; }
            remove { _onLoad -= value; }
        }

        #endregion
    }
}