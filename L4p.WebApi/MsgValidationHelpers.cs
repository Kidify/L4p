using System;
using System.Linq.Expressions;
using L4p.Common.Extensions;

namespace L4p.WebApi
{
    public class MsgValidationHelpers
    {
        protected static string get_getter_name(Expression getter)
        {
            string name = getter.ToString();
            int from = name.LastIndexOf('.');

            return
                name.Substring(from + 1);
        }

        protected static void fail_if_null(string value, Expression<Func<string>> getter)
        {
            if (value.IsNotEmpty())
                return;

            throw new BadRequestException(
                "field can't be empty '{0}'", get_getter_name(getter));
        }

        protected static void fail_if_null<T>(T value, Expression<Func<T>> getter)
            where T : class
        {
            if (value != null)
                return;

            throw new BadRequestException(
                "field can't be empty '{0}'", get_getter_name(getter));
        }

        protected static void fail_if_zero<T>(T value, Expression<Func<T>> getter)
            where T : struct, IComparable<T>
        {
            if (0 != value.CompareTo(default(T)))
                return;

            throw new BadRequestException(
                "field can't be zero '{0}'", get_getter_name(getter));
        }
    }
}