using System;
using Newtonsoft.Json;
using L4p.Common.IoCs;
using L4p.Common.Json;
using L4p.Common.Loggers;
using L4p.Common.PubSub.utils;

namespace L4p.Common.PubSub.client
{
    interface IJsonEngine
    {
        string MsgToJson(object msg);
        object MsgFromJson(Topic topic, string json);
    }

    class JsonEngine : IJsonEngine
    {
        #region members

        private readonly ILogFile _log;

        #endregion

        #region construction

        public static IJsonEngine New(IIoC ioc)
        {
            return
                new JsonEngine(ioc);
        }

        private JsonEngine(IIoC ioc)
        {
            _log = ioc.Resolve<ILogFile>();
        }

        #endregion

        #region private
        #endregion

        #region interface

        string IJsonEngine.MsgToJson(object msg)
        {
            return
                msg.AsSingleLineJson();
        }

        object IJsonEngine.MsgFromJson(Topic topic, string json)
        {
            var type = topic.Type;

            try
            {
                return
                    JsonConvert.DeserializeObject(json, type);
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Failed to parse type '{0}' of topic '{1}' from {2}", type.Name, topic.Name, json);
                throw;
            }
        }

        #endregion
    }
}