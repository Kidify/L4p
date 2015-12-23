using System;
using System.Diagnostics;
using System.ServiceModel.Channels;

namespace L4p.Common.ConfigurationFiles
{
    public interface IConfigRa2<TConfig>
        where TConfig : class, new()
    {

        bool IsUpToDate();
        TConfig Values { get; }
    }

    public class ConfigRa2<TConfig> : IConfigRa2<TConfig>
        where TConfig : class, new()
    {
        #region members

        private readonly TimeSpan _onChangeLatency;  // latency
        private readonly string _configKey;
        private readonly Type _classType;
        private readonly ICraManager _cram;
        private readonly Stopwatch _tm;

        private TConfig _values;

        #endregion

        #region construction

        public static IConfigRa2<TConfig> New(string configKey)
        {
            return
                new ConfigRa2<TConfig>(configKey);
        }

        private ConfigRa2(string configKey)
        {
            var classType = typeof(TConfig);
            var cram = CraManager.Instance;

            _onChangeLatency = TimeSpan.MinValue;
            _cram = cram;
            _configKey = configKey;
            _classType = classType;
            _tm = Stopwatch.StartNew();

            _values = read_or_make_values(cram, configKey);
        }

        #endregion

        #region private

        private static TConfig read_or_make_values(ICraManager cram, string configKey)
        {
            var values = 
                cram.ReadConfig<TConfig>(configKey) ?? new TConfig();

            return values;
        }

        private TConfig get_values()
        {
            if (_tm.Elapsed < _onChangeLatency)
                return _values;

            _values = read_or_make_values(_cram, _configKey);
            _tm.Restart();

            return _values;
        }

        #endregion

        #region interface

        bool IConfigRa2<TConfig>.IsUpToDate()
        {
            if (_tm.Elapsed < _onChangeLatency)
                return true;

            var isUpToDate = _cram.IsUpToDate<TConfig>(_configKey);

            return isUpToDate;
        }

        TConfig IConfigRa2<TConfig>.Values
        {
            get { return get_values(); }
        }

        #endregion
    }
}