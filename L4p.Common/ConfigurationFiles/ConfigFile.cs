using System;
using System.IO;
using L4p.Common.Extensions;
using L4p.Common.Helpers;
using L4p.Common.Loggers;

namespace L4p.Common.ConfigurationFiles
{
    public interface IConfigFile<T>
        where T : class, new()
    {
        bool IsUpToDate();
        T Read();

        string Path { get; }
        string Name { get; }
    }

    public class ConfigFile<T> : IConfigFile<T>
        where T : class, new()
    {
        #region config

        public class Config
        {
            public TimeSpan MinimalCheckSpan { get; set; }

            public Config()
            {
                MinimalCheckSpan = 2.Seconds();
            }
        }

        #endregion

        #region members

        private readonly string _path;
        private readonly string _name;
        private readonly Config _config;
        private readonly ILogFile _log;

        private DateTime _loadedAt;
        private DateTime _checkedAt;
        private T _configuration;

        #endregion

        #region construction

        public static IConfigFile<T> New(string path, ILogFile log = null, Config config = null)
        {
            if (log == null)
                log = LogFile.New("bootstrap.log");

            return
                new ConfigFile<T>(path, log, config ?? new Config());
        }

        private ConfigFile(string path, ILogFile log, Config config)
        {
            _path = path;
            _name = Path.GetFileName(path);
            _config = config;
            _log = log;
            _loadedAt = DateTime.MinValue;
            _checkedAt = DateTime.MinValue;
            _configuration = new T();
        }

        #endregion

        #region private

        private T read_configuration(string path)
        {
            string json = Try.Catch.Wrap(
                () => File.ReadAllText(path),
                ex => ex.WrapWith<ConfigFileException>("Failed to read json from configuration file '{0}'", path));

            var parser = Json2Config<T>.New(path);

            var configuration = Try.Catch.Wrap(
                () => parser.ParseJson(path, json, _log),
                ex => ex.WrapWith<ConfigFileException>("Failed to parse json from configuration file '{0}'", path));

            _log.Info("Configuration file '{0}' is loaded (and parsed)", path);

            return configuration;
        }

        #endregion

        #region IConfigFile

        bool IConfigFile<T>.IsUpToDate()
        {
            var now = DateTime.UtcNow;

            var sinceLastCheck = now - _checkedAt;

            if (sinceLastCheck <= _config.MinimalCheckSpan)
                return true;

            var lastUpdatedAt = File.GetLastWriteTimeUtc(_path);
            _checkedAt = now;

            bool isUpToDate = 
                lastUpdatedAt <= _loadedAt;

            return isUpToDate;
        }

        T IConfigFile<T>.Read()
        {
            var now = DateTime.UtcNow;

            var lastUpdatedAt = File.GetLastWriteTimeUtc(_path);
            _checkedAt = now;

            if (lastUpdatedAt > _loadedAt)
            {
                Try.Catch.Handle(
                    () => _configuration = read_configuration(_path),
                    ex => _log.Error(ex, "While loading configuration from '{0}'", _path));

                _loadedAt = now;
            }

            Validate.NotNull(_configuration);
            return _configuration;
        }

        string IConfigFile<T>.Path
        {
            get { return _path; }
        }

        string IConfigFile<T>.Name
        {
            get { return _name; }
        }

        #endregion
    }
}