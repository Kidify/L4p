using System;
using L4p.Common.ConfigurationFiles;
using L4p.Common.Extensions;
using L4p.Common.IoCs;
using L4p.Common.Loggers;

namespace L4p.Common.PubSub.client
{
    interface ISignalsConfigRa
    {
        SignalsConfig Values { get; }
        string MakeHubUri();
    }

    class SignalsConfigRa : ISignalsConfigRa
    {
        #region members

        private readonly SignalsConfig _config;

        #endregion

        #region construction

        public static ISignalsConfigRa New(SignalsConfig config)
        {
            return
                new SignalsConfigRa(config);
        }

        private SignalsConfigRa(SignalsConfig config)
        {
            _config = config;
        }

        #endregion

        #region private
        #endregion

        #region interface

        SignalsConfig ISignalsConfigRa.Values
        {
            get { return _config; }
        }

        string ISignalsConfigRa.MakeHubUri()
        {
            var port = _config.Port;
            var host = _config.HubHost;
            var pattern = _config.HubUri;

            var uri = pattern.Fmt(host, port);

            return uri;
        }

        #endregion
    }
}