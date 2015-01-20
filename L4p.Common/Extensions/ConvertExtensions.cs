using System;
using L4p.Common.Helpers;

namespace L4p.Common.Extensions
{
    public static class ConvertExtensions
    {
        public static T ConvertTo<T>(this object value)
        {
            Type type = typeof (T);

            try
            {
                return
                    (T) Convert.ChangeType(value, type);
            }
            catch (Exception ex)
            {
                throw
                    new InvalidCastException(
                        "Can't convert value='{0}' to '{1}' type".Fmt(value, type.Name), ex);
            }
        }

        public static bool? AsNullableBool(this string value)
        {
            if (value.IsEmpty())
                return null;

            switch (value)
            {
                case "0": return false;
                case "1": return true;
            }

            if (value == Boolean.FalseString) 
                return false;

            if (value == Boolean.TrueString) 
                return true;

            throw new InvalidCastException(
                "Can't convert value='{0}' to 'bool?' type".Fmt(value));
        }

        public static T As<T>(this string value)
        {
            Type type = typeof(T);

            try
            {
                if (type == typeof(Guid))
                {
                    return 
                        (T) (object) Guid.Parse(value);
                }

                if (type == typeof (Guid?))
                {
                    if (value == null) 
                        return (T) (object) null;

                    return
                        (T) (object) Guid.Parse(value);
                }

                if (type == typeof(bool))
                {
                    if (value == "0") return (T) (object) false;
                    if (value == "1") return (T) (object) true;
                    if (value == Boolean.FalseString) return (T) (object) false;
                    if (value == Boolean.TrueString) return (T) (object) true;
                }

                return
                    (T) Convert.ChangeType(value, type);
            }
            catch (Exception ex)
            {
                throw
                    new InvalidCastException(
                        "Can't convert value='{0}' to '{1}' type".Fmt(value, type.Name), ex);
            }
        }
    }
}