using System;
using Newtonsoft.Json;
using L4p.Common.Extensions;
using L4p.Common.Helpers;

namespace L4p.Common.ConfigurationFiles
{
    public class TimeSpanFromJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TimeSpan);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type type, object value, JsonSerializer serializer)
        {
            var str = reader.Value as string;

            if (str == "24:00")
                return 24.Hours();

            var span = Try.Catch.Wrap(
                () => TimeSpan.Parse(str),
                ex => ex.WrapWith<InvalidCastException>("Failed to parse value '{0}' (type='{1}') as TimeSpan", str, type.Name));

            return span;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }
    }
}