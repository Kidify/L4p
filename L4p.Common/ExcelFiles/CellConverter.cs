using System.Linq;
using L4p.Common.Extensions;

namespace L4p.Common.ExcelFiles
{
    delegate object ConvertValue(string value);

    interface ICellConverter
    {
        int convert_int(string value);
        double convert_double(string value);
        decimal convert_decimal(string value);
        string convert_string(string value);
        bool convert_bool(string value);
        int[] convert_int_array(string value);
        double[] convert_double_array(string value);
        decimal[] convert_decimal_array(string value);
        string[] convert_string_array(string value);
        int? convert_nullable_int(string value);
        double? convert_nullable_double(string value);
        decimal? convert_nullable_decimal(string value);
    }

    class CellConverter : ICellConverter
    {
        #region members
        #endregion

        #region construction

        public static ICellConverter New()
        {
            return
                new CellConverter();
        }

        private CellConverter()
        {
        }

        #endregion

        #region private
        #endregion

        #region interface
        #endregion

        int ICellConverter.convert_int(string value)
        {
            if (value.IsEmpty())
                return 0;

            return
                int.Parse(value);
        }

        double ICellConverter.convert_double(string value)
        {
            if (value.IsEmpty())
                return 0;

            return
                double.Parse(value);
        }

        decimal ICellConverter.convert_decimal(string value)
        {
            if (value.IsEmpty())
                return 0;

            return
                decimal.Parse(value);
        }

        string ICellConverter.convert_string(string value)
        {
            return value;
        }

        bool ICellConverter.convert_bool(string value)
        {
            if (value.IsEmpty())
                return false;

            return
                bool.Parse(value);
        }

        int[] ICellConverter.convert_int_array(string value)
        {
            if (value.IsEmpty())
                return null;

            var array = value.Split(',');

            var query =
                from x in array
                select int.Parse(x);

            return
                query.ToArray();
        }

        double[] ICellConverter.convert_double_array(string value)
        {
            if (value.IsEmpty())
                return null;

            var array = value.Split(',');

            var query =
                from x in array
                select double.Parse(x);

            return
                query.ToArray();
        }

        decimal[] ICellConverter.convert_decimal_array(string value)
        {
            if (value.IsEmpty())
                return null;

            var array = value.Split(',');

            var query =
                from x in array
                select decimal.Parse(x);

            return
                query.ToArray();
        }

        string[] ICellConverter.convert_string_array(string value)
        {
            if (value.IsEmpty())
                return null;

            var array = value.Split(',');
            return array;
        }

        int? ICellConverter.convert_nullable_int(string value)
        {
            if (value.IsEmpty())
                return null;

            return
                int.Parse(value);
        }

        double? ICellConverter.convert_nullable_double(string value)
        {
            if (value.IsEmpty())
                return null;

            return
                double.Parse(value);
        }

        decimal? ICellConverter.convert_nullable_decimal(string value)
        {
            if (value.IsEmpty())
                return null;

            return
                decimal.Parse(value);
        }
   }

}