using System;
using System.Collections.Generic;
using System.Linq;
using L4p.Common.Helpers;

namespace L4p.Common
{
    public static class Enum<T>
        where T : struct
    {
        public static bool IsDefined(string name)
        {
            return 
                Enum.IsDefined(typeof(T), name);
        }

        public static bool IsDefined(int value)
        {
            return 
                Enum.IsDefined(typeof(T), value);
        }

        public static T From(int value)
        {
            object obj = value;

            if (!Enum.IsDefined(typeof(T), obj))
                throw new RuntimeValidationException("Failed to convert int value {0} to enum '{1}'", value, typeof(T).Name);

            return (T) obj;
        }

        public static T From<TOther>(TOther value)
            where TOther : struct
        {
            object obj = (int) (object) value;

            if (!Enum.IsDefined(typeof(T), obj))
                throw new RuntimeValidationException("Failed to convert value '{0}' of type '{1}' to enum '{2}'", value, typeof(TOther).Name, typeof(T).Name);

            return (T) obj;
        }

        public static T From(string str)
        {
            T value;

            if (!Enum.TryParse(str, out value))
                throw new RuntimeValidationException("Failed to convert string '{0}' to enum '{1}'", str, typeof (T).Name);

            if (!Enum.IsDefined(typeof(T), value))
                throw new RuntimeValidationException("Failed to convert string '{0}' to enum '{1}'", str, typeof(T).Name);

            return value;
        }

        public static IEnumerable<T> GetValues()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}