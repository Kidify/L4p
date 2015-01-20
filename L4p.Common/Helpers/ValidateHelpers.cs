using System;
using System.Collections.Generic;
using L4p.Common.Extensions;

namespace L4p.Common.Helpers
{
    public class RuntimeValidationException : L4pException
    {
        public RuntimeValidationException(string msg, Exception cause)
            : base(msg, cause)
        { }

        public RuntimeValidationException(string msg, params object[] args)
            : base(msg, args)
        {}

        public RuntimeValidationException(Exception cause, string msg, params object[] args)
            : base(cause, msg, args)
        {}
    }

    public static class Validate
    {
        private static void validate(bool expr, string errMsg, params object[] args)
        {
            if (expr)
                return;

            throw
                new RuntimeValidationException("Runtime validation: {0}", errMsg.Fmt(args));
        }

        private static void validate(bool expr, Func<Exception> onFailure)
        {
            if (expr)
                return;

            throw onFailure();
        }

        public static void NotNull(object log, object value) { validate(value != null, "argument should not be null"); }

        public static void That(bool expr) { validate(expr, "expression should be true"); }
        public static void That(bool expr, string fmt, params object[] args) { validate(expr, fmt.Fmt(args)); }
        public static void NotNull(object value) { validate(value != null, "argument should not be null"); }
        public static void NotNull(int value) { validate(value != 0, "argument should not be zero"); }
        public static void NotZero(int value) { validate(value != 0, "argument should not be zero"); }
        public static void NotZero(double value) { validate(Math.Abs(value - 0) > double.Epsilon, "argument should not be zero"); }
        public static void NotZero(decimal value) { validate(value != 0, "argument should not be zero"); }
        public static void NotDefault<T>(T value) where T : IComparable { validate(value.CompareTo(default(T)) != 0, "value should not be empty"); }
        public static void False(bool value) { validate(!value, "argument should be false"); }
        public static void NotNullNorEmpty(string value) {validate(!String.IsNullOrWhiteSpace(value), "string should not be null nor empty");}

        public static void That(Func<bool> expr, Func<Exception> onFailure) { validate(expr(), onFailure); }
        public static void NotNull(Func<object> value, Func<Exception> onFailure) { validate(value() != null, onFailure); }
        public static void NotNull(Func<int> value, Func<Exception> onFailure) { validate(value() != 0, onFailure); }
        public static void NotZero(Func<int> value, Func<Exception> onFailure) { validate(value() != 0, onFailure); }
        public static void False(Func<bool> value, Func<Exception> onFailure) { validate(!value(), onFailure); }
        public static void NotNullNorEmpty(Func<string> value, Func<Exception> onFailure) {validate(!String.IsNullOrWhiteSpace(value()), onFailure);}

        public static void NotEmpty(string str)
        {
            validate(str.IsNotEmpty(), "string is null or empty");
        }

        public static void NotEmpty<T>(T[] array)
        {
            validate(array != null, "array is null");
            validate(array.Length > 0, "array is empty");
        }

        public static void NotEmpty<T>(List<T> list)
        {
            validate(list != null, "list is null");
            validate(list.Count > 0, "list is empty");
        }

        public static void Fail() { validate(false, "Precondition has failed"); }
    }
}