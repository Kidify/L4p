using System;
using System.Collections.Generic;
using System.Reflection;
using L4p.Common.Extensions;
using L4p.Common.Helpers;

namespace L4p.Common.ExcelFiles
{
    class PropertySetter<T>
    {
        public delegate void ImplMethod(T self, string value);

        public string DeclarationStr { get; set; }
        public ImplMethod Impl { get; set; }
    }

    interface IReflectedType<T>
        where T : class
    {
        PropertySetter<T> GetSetter(string property);
    }

    class ReflecedType<T> : IReflectedType<T>
        where T : class
    {
        private readonly Type _type;
        private readonly string _name;
        private readonly Dictionary<Type, ConvertValue> _converters;
        private readonly Dictionary<string, PropertySetter<T>> _setters;

        #region members
        #endregion

        #region construction

        public static IReflectedType<T> New(ICellConverter converter = null)
        {
            converter = converter ?? CellConverter.New();

            return
                new ReflecedType<T>(converter);
        }

        private ReflecedType(ICellConverter converter)
        {
            _type = typeof(T);
            _name = _type.Name;

            _converters = new Dictionary<Type, ConvertValue> {
                { typeof(int), v => converter.convert_int(v) },
                { typeof(double), v => converter.convert_double(v) },
                { typeof(decimal), v => converter.convert_decimal(v) },
                { typeof(string), converter.convert_string },
                { typeof(bool), v => converter.convert_bool(v) },
                { typeof(int[]), converter.convert_int_array },
                { typeof(double[]), converter.convert_double_array },
                { typeof(decimal[]), converter.convert_decimal_array },
                { typeof(string[]), converter.convert_string_array },
                { typeof(int?), v => converter.convert_nullable_int(v) },
                { typeof(double?), v => converter.convert_nullable_double(v) },
                { typeof(decimal?), v => converter.convert_nullable_decimal(v) },
            };

            _setters = new Dictionary<string, PropertySetter<T>>(StringComparer.InvariantCultureIgnoreCase);

            fill_setters();
        }

        #endregion

        #region private

        private void set_property(PropertyInfo property, object self, object value)
        {
            property.SetValue(self, value, null);
        }

        private void fill_setters()
        {
            var properties = _type.GetProperties();

            foreach (var property_ in properties)
            {
                var property = property_;

                var name = property.Name;
                var type = property.PropertyType;

                ConvertValue converter;

                if (!_converters.TryGetValue(type, out converter))
                    continue;

                Validate.NotNull(converter);

                PropertySetter<T>.ImplMethod impl =
                    (self, value) => set_property(property, self, converter(value));

                var declarationStr = "{0} {1}.{2}".Fmt(type.Name, _type.Name, name);

                var setter = new PropertySetter<T>{
                    DeclarationStr = declarationStr,
                    Impl = impl
                };

                _setters[name] = setter;
            }
        }

        #endregion

        #region interface

        PropertySetter<T> IReflectedType<T>.GetSetter(string property)
        {
            PropertySetter<T> setter;

            _setters.TryGetValue(property, out setter);
            return setter;
        }

        #endregion
    }
}