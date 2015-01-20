using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using L4p.Common.Extensions;
using L4p.Common.Helpers;

namespace L4p.Common.FunnelsModel
{
    interface IJsonPort
    {
        string ToWireJson<T>(T obj);
        string ToReadableJson<T>(T obj);
        T FromJson<T>(string json);
    }

    class JsonPort : IJsonPort
    {
        #region members
        #endregion

        #region construction

        public static IJsonPort New()
        {
            return
                new JsonPort();
        }

        private JsonPort()
        {
        }

        #endregion

        #region private

        private string serialize_json<T>(Func<string> convert)
        {
            Type type = typeof(T);

            return Try.Catch.Wrap(
                () => convert(),
                ex => ex.WrapWith<FunnelsException>("Failed to serialize type '{0}'", type.Name));
        }

        private T deserialize_json<T>(string json, Func<T> convert)
        {
            Type type = typeof(T);

            return Try.Catch.Wrap(
                () => convert(),
                ex => ex.WrapWith<FunnelsException>("Failed to deserialize type '{0}' from '{1}'", type.Name, json));
        }

        #endregion

        #region IJsonPort

        string IJsonPort.ToWireJson<T>(T obj)
        {
            string json = serialize_json<T>(
                () => JsonConvert.SerializeObject(obj));

            return json;
        }

        string IJsonPort.ToReadableJson<T>(T obj)
        {
            string json = serialize_json<T>(
                () => JsonConvert.SerializeObject(obj, Formatting.Indented, new StringEnumConverter()));

            return json;
        }

        T IJsonPort.FromJson<T>(string json)
        {
            T obj = deserialize_json(json,
                () => JsonConvert.DeserializeObject<T>(json));

            return obj;
        }

        #endregion
    }

}