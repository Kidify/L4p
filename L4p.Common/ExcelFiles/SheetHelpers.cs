using System;
using OfficeOpenXml;
using L4p.Common.Extensions;
using L4p.Common.Helpers;

namespace L4p.Common.ExcelFiles
{
    static class SheetHelpers
    {
        public static string read_cell_value(this ExcelRange cells, int rowNo, int colNo)
        {
            var cell = Try.Catch.Wrap(
                () => cells[rowNo, colNo],
                ex => ex.WrapWith<ExcelFileException>("Failed to get excel cell[{0}:{1}]", rowNo, colNo));

            var value = Try.Catch.Wrap(
                () => cell.GetValue<string>(),
                ex => ex.WrapWith<ExcelFileException>("Failed to get excel value of cell[{0}:{1}]", rowNo, colNo));

            return value;
        }

        public static SheetLine read_single_row(this ExcelWorksheet excel, int rowNo, int colCount)
        {
            var cells = excel.Cells;
            var values = new string[colCount];

            for (int indx = 0; indx < colCount; indx++)
            {
                var colNo = indx + 1;
                var value = read_cell_value(cells, rowNo, colNo);

                values[indx] = value;
            }

            var opCode = values[0] ?? String.Empty;

            var line = new SheetLine
            {
                RowNo = rowNo,
                OpCode = opCode,
                Values = values
            };

            return line;
        }

        public static void fail_if<T>(this SheetData<T> sheet, bool expr, string fmt, params object[] args)
            where T : class, new()
        {
            if (!expr)
                return;

            var errMsg = fmt.Fmt(args);

            throw 
                new ExcelFileException(errMsg);
        }
    }
}