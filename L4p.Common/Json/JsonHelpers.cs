using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using L4p.Common.Extensions;

namespace L4p.Common.Json
{
    public static class JsonHelpers
    {
        private static readonly JsonConverter[] _readableConverters = {
            new IsoDateTimeConverter { DateTimeFormat = "dd-MMM-yyyy HH:mm:ss.fff" },
            new StringEnumConverter()
        };

        public static string AsReadableJson<T>(this T data)
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented, _readableConverters);
            return json;
        }

        public static string AsSingleLineJson<T>(this T data)
        {
            string json = JsonConvert.SerializeObject(data, _readableConverters);
            return json;
        }

        public static string ToJson<T>(this T self, Formatting formatting = Formatting.Indented)
        {
            var json = JsonConvert.SerializeObject(self, formatting, _readableConverters);
            return json;
        }

        public static T FromJson<T>(string json)
        {
            if (json.IsEmpty())
                return default(T);

            var data = JsonConvert.DeserializeObject<T>(json);
            return data;
        }

        public static object FromJson(string json, Type type)
        {
            if (json.IsEmpty())
                return null;

            var data = JsonConvert.DeserializeObject(json, type);
            return data;
        }
    }
}