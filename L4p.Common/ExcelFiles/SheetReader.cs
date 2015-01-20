using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using OfficeOpenXml;
using L4p.Common.Extensions;
using L4p.Common.Helpers;
using L4p.Common.Json;
using L4p.Common.Loggers;

namespace L4p.Common.ExcelFiles
{
    public class SheetLine
    {
        public int RowNo { get; set; }
        public string OpCode { get; set; }
        public string[] Values { get; set; }
    }

    class SheetData<T>
        where T : class, new()
    {
        public int ColCount { get; set; }
        public int RowCount { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }

        public T[] Items { get; set; }
        public string[] Properties { get; set; }
        public string[] Titles { get; set; }
        public SheetLine[] Lines { get; set; }
        public SheetLine[] RawItems { get; set; }
        public PropertySetter<T>[] Setters { get; set; }
        public IDictionary<string, int> ColumnName2ItsIndx { get; set; }
        public IDictionary<int, int> ItemIndx2RowNo { get; set; }
    }

    interface ISheetReader<T>
        where T : class, new()
    {
        SheetData<T> ReadSheetData(string path, ExcelWorksheet excel);
    }

    class SheetReader<T> : ISheetReader<T>
        where T : class, new()
    {
        #region members

        private readonly ILogFile _log;
        private readonly ExcelFileConfig _config;
        private readonly IReflectedType<T> _t;

        #endregion

        #region construction

        public static ISheetReader<T> New(ExcelFileConfig config, ILogFile log = null)
        {
            return
                new SheetReader<T>(config, log ?? LogFile.Null);
        }

        private SheetReader(ExcelFileConfig config, ILogFile log)
        {
            _log = log;
            _config = config;
            _t = ReflecedType<T>.New();
        }

        #endregion

        #region private

        private static bool row_is_empty(SheetLine line)
        {
            return
                !line.Values.Any(value => value.IsNotEmpty());
        }

        private void read_properties(SheetLine line, SheetData<T> sheet)
        {
            int rowNo = line.RowNo;

            sheet.fail_if(sheet.Properties != null, 
                "Second properties definition at row {0}", rowNo);

            var properties =
                from value in line.Values
                select value;

            sheet.Properties = properties.ToArray();
        }

        private void read_titles(SheetLine line, SheetData<T> sheet)
        {
            int rowNo = line.RowNo;

            sheet.fail_if(sheet.Titles != null, 
                "Second titels definition at row {0}", rowNo);
            
            sheet.Titles = line.Values;
        }

        private SheetData<T> read_excel_sheet(string path, ExcelWorksheet excel)
        {
            int colCount = excel.Dimension.End.Column;
            int rowCount = excel.Dimension.End.Row;

            var sheet = new SheetData<T>
            {
                ColCount = colCount,
                RowCount = rowCount,
                FileName = Path.GetFileName(path),
                FilePath = path
            };

            var lines = new List<SheetLine>();
            var items = new List<SheetLine>();
            var itemIndx2RowNo = new Dictionary<int, int>();

            for (int rowNo = 1; rowNo <= rowCount; rowNo++)
            {
                var line = excel.read_single_row(rowNo, colCount);

                if (row_is_empty(line))
                    continue;

                lines.Add(line);

                var opCode = line.OpCode;
                Validate.NotNull(opCode);

                switch (opCode)
                {
                    case ";":
                        break;

                    case ExcelFileConfig.Const.OpCode:
                        read_properties(line, sheet);
                        break;

                    case ExcelFileConfig.Const.Titles:
                        read_titles(line, sheet);
                        break;

                    case "":
                        itemIndx2RowNo[items.Count()] = rowNo;
                        items.Add(line);
                        break;

                    default:
                        throw new ExcelFileException("Unsupported OpCode '{0}' at row {1}", opCode, rowNo);
                }
            }

            sheet.Lines = lines.ToArray();
            sheet.RawItems = items.ToArray();
            sheet.ItemIndx2RowNo = itemIndx2RowNo;

            return sheet;
        }

        private void validate_sheet(SheetData<T> sheet)
        {
            sheet.fail_if(sheet.Properties.IsEmpty(), "row with definitions of properties ('{0}') is missing", ExcelFileConfig.Const.OpCode);
            sheet.fail_if(sheet.Properties.IsEmpty(), "row with titles ('{0}') is missing", ExcelFileConfig.Const.Titles);
        }

        private void build_reflection_map(SheetData<T> sheet)
        {
            var propCount = sheet.Properties.Length;
            var setters = new PropertySetter<T>[propCount];

            for (int indx = 0; indx < propCount; indx++)
            {
                var property = sheet.Properties[indx];

                if (property.IsEmpty())
                    continue;

                var setter = _t.GetSetter(property);
                setters[indx] = setter;
            }

            sheet.Setters = setters;
        }

        private void build_substitution_map(SheetData<T> sheet)
        {
            var propCount = sheet.Properties.Length;
            var map = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

            for (int indx = 0; indx < propCount; indx++)
            {
                var property = sheet.Properties[indx];
                map[property] = indx;
            }

            sheet.ColumnName2ItsIndx = map;
        }

        private void set_value(string fileName, SheetLine line, string property, T self, string value, PropertySetter<T> setter)
        {
            if (setter == null)
                return;

            if (setter.Impl == null)
                return;

            try
            {
                setter.Impl(self, value);
            }
            catch (Exception ex)
            {
                _log.Error("'{0}' at row {1}: failed to convert column '{2}' value '{3}' into '{4}'; {5}",
                    fileName, line.RowNo, property, value, setter.DeclarationStr, ex.Message);
            }
        }

        private T build_single_item(SheetLine line, SheetData<T> sheet)
        {
            var count = line.Values.Length;
            var item = new T();

            for (int indx = 0; indx < count; indx++)
            {
                var property = sheet.Properties[indx];
                var value = line.Values[indx];
                var setter = _t.GetSetter(property);

                set_value(sheet.FileName, line, property, item, value, setter);
            }

            return item;
        }

        private void parse_items(SheetData<T> sheet)
        {
            var items = new List<T>();

            foreach (var line in sheet.RawItems)
            {
                var item = build_single_item(line, sheet);

                if (item == null)
                    continue;

                items.Add(item);
            }

            sheet.Items = items.ToArray();
        }

        private void link_sheet(SheetData<T> sheet)
        {
        }

        #endregion

        #region interface

        public SheetData<T> ReadSheetData(string path, ExcelWorksheet excel)
        {
            var sheet = read_excel_sheet(path, excel);
            validate_sheet(sheet);
            build_reflection_map(sheet);
            build_substitution_map(sheet);
            parse_items(sheet);
            link_sheet(sheet);

            return sheet;
        }

        #endregion
    }
}