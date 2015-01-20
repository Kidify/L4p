using System;
using System.Text.RegularExpressions;
using L4p.Common.Extensions;
using L4p.Common.Helpers;
using L4p.Common.Loggers;

namespace L4p.Common.ExcelFiles
{
    public interface ISingleSheet<T>
    {
        int RowCount { get; }
        int ColCount { get; }
        T[] Items { get; }
        SheetLine[] Lines { get; }
        string[] Titles { get; }
        string FilePath { get; }
        string FileName { get; }

        string MapColumnsToValues(int itemIndx, string str);
    }

    class SingleSheet<T> : ISingleSheet<T>
        where T : class, new()
    {
        #region members

        private readonly ILogFile _log;
        private readonly SheetData<T> _data;

        #endregion

        #region construction

        public static ISingleSheet<T> New(SheetData<T> data, ILogFile log = null)
        {
            return
                new SingleSheet<T>(data, log ?? LogFile.Null);
        }

        private SingleSheet(SheetData<T> data, ILogFile log)
        {
            Validate.NotNull(data);

            _log = log;
            _data = data;
        }

        #endregion

        #region private

        private int get_rowNo_by_itemIndx(int itemIndx)
        {
            int rowNo;

            if (!_data.ItemIndx2RowNo.TryGetValue(itemIndx, out rowNo))
                throw new ExcelFileException("Invalid itemIndx {0}", itemIndx);

            return rowNo;
        }

        private int get_column_no_by_name(string columnName)
        {
            int columnNo;

            if (!_data.ColumnName2ItsIndx.TryGetValue(columnName, out columnNo))
                return -1;

            return columnNo;
        }

        #endregion

        #region interface

        int ISingleSheet<T>.RowCount        { get { return _data.RowCount; } }
        int ISingleSheet<T>.ColCount        { get { return _data.ColCount; } }
        T[] ISingleSheet<T>.Items           { get { return _data.Items; } }
        SheetLine[] ISingleSheet<T>.Lines   { get { return _data.Lines; } }
        string[] ISingleSheet<T>.Titles     { get { return _data.Titles; } }
        string ISingleSheet<T>.FilePath     { get { return _data.FilePath; } }
        string ISingleSheet<T>.FileName     { get { return _data.FileName; } }

        string ISingleSheet<T>.MapColumnsToValues(int itemIndx, string str)
        {
            if (str.IsEmpty())
                return str;

            var result = str;

            var rowNo = get_rowNo_by_itemIndx(itemIndx);

            foreach (Match match in Regex.Matches(str, "{(.*?)}"))
            {
                var prm = match.Value;
                var columnName = prm.Substring(1, prm.Length-2).Trim();
                var columnNo = get_column_no_by_name(columnName);

                if (columnNo == -1)
                {
                    _log.Warn("Failed to substitute '{0}' at row {1} (no such column is found)", prm, rowNo);
                    continue;
                }

                var value = _data.Lines[rowNo - 1].Values[columnNo];
                result = result.Replace(prm, value);
            }

            return result;
        }

        #endregion
    }
}