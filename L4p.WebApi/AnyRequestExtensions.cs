using System;
using System.Linq.Expressions;
using L4p.Common.Extensions;

namespace L4p.WebApi
{
    public class AnyRequestExtensions
    {
        private string get_getter_name(Expression getter)
        {
            string name = getter.ToString();
            int from = name.LastIndexOf('.');

            return
                name.Substring(@from + 1);
        }

        public void if_false(bool expr, string fmt, params object[] args)
        {
            if (expr)
                return;

            throw 
                new BadRequestException(fmt, args);
        }

        public void if_zero(int value, Expression<Func<int>> getter)
        {
            if (value != 0)
                return;

            throw new BadRequestException(
                "field can't be zero '{0}'", get_getter_name(getter));
        }

        public void if_zero(decimal value, Expression<Func<decimal>> getter)
        {
            if (value != 0)
                return;

            throw new BadRequestException(
                "field can't be zero '{0}'", get_getter_name(getter));
        }

        public void if_default(DateTime value, Expression<Func<DateTime>> getter)
        {
            if (value != DateTime.MinValue)
                return;

            throw new BadRequestException(
                "field should be set '{0}'", get_getter_name(getter));
        }

        public void if_default(TimeSpan value, Expression<Func<TimeSpan>> getter)
        {
            if (value != TimeSpan.MinValue)
                return;

            throw new BadRequestException(
                "field should be set '{0}'", get_getter_name(getter));
        }

        public void if_empty(string value, Expression<Func<string>> getter)
        {
            if (value.IsNotEmpty())
                return;

            throw new BadRequestException(
                "field can't be empty '{0}'", get_getter_name(getter));
        }

        public void if_bad_country_code(string value, Expression<Func<string>> getter)
        {
            if (value.IsEmpty())
                return;

            if (value.Length == 2)
                return;

            throw new BadRequestException(
                "Bad country code in '{0}'; should be a two letter ISO code", get_getter_name(getter));
        }
    }
}