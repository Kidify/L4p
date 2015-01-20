using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using L4p.Common.Extensions;

namespace L4p.Common.CountersAccumulators
{
    class CountersFormatter
    {
        class Entry
        {
            public string Name { get; set; }
            public object Value { get; set; }
        }

        private static readonly object SystemAssembly = typeof(string).Assembly;

        private readonly List<Entry> _values;

        public CountersFormatter(Type type, object counters)
        {
            _values = new List<Entry>();
            parse_values(type.Name, type, counters);
            sort_values();
        }

        private void parse_properties(string path, Type type, object counters)
        {
            var props =
                from
                    prop in type.GetProperties()
                where
                    !prop.PropertyType.IsClass ||
                    prop.PropertyType == typeof(string)
                select
                    prop;

            foreach (var prop in props)
            {
                object value = prop.GetValue(counters, null);

                var entry = new Entry
                    {
                        Name = "{0}.{1}".Fmt(path, prop.Name),
                        Value = value
                    };

                _values.Add(entry);
            }
        }

        private void parse_inner_counters(string path, Type type, object counters)
        {
            var props =
                from
                    prop in type.GetProperties()
                where
                    prop.PropertyType.IsClass &&
                    !ReferenceEquals(prop.PropertyType.Assembly, SystemAssembly)
                select
                    prop;

            foreach (var prop in props)
            {
                object value = prop.GetValue(counters, null);
                string innerPath = "{0}.{1}".Fmt(path, prop.Name);
                Type innerType = value.GetType();

                parse_values(innerPath, innerType, value);
            }
        }

        private void parse_values(string path, Type type, object counters)
        {
            parse_properties(path, type, counters);
            parse_inner_counters(path, type, counters);
        }

        private void sort_values()
        {
            _values.Sort(
                (x, y) => String.Compare(x.Name, y.Name, StringComparison.InvariantCulture));
        }

        public string Format(string prefix, Func<object, string> formatName)
        {
            var sb = new StringBuilder();
            bool firstEntry = true;

            foreach (var entry in _values)
            {
                if (firstEntry)
                {
                    firstEntry = false;
                }
                else
                {
                    sb.AppendLine();
                }

                string name = formatName(entry.Name);
                sb.AppendFormat("{0}{1} {2}", prefix, name, entry.Value);
            }

            return sb.ToString();
        }
    }

    public class CountersHelpers
    {
        public static string Format<T>(T counters, string prefix = "", int nameFieldLength = 0)
            where T : class
        {
            var type = typeof(T);
            var formatter = new CountersFormatter(type, counters);

            string namePattern = "{{0,-{0}}}".Fmt(nameFieldLength);

            Func<object, string> formatName =
                name => namePattern.Fmt(name);

            return
                formatter.Format(prefix, formatName);
        }
    }
}