using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using L4p.Common.Extensions;
using L4p.Common.Helpers;
using L4p.Common.Loggers;

namespace L4p.Common.ExcelFiles
{
    public interface IExcelFile<T>
        where T : class, new()
    {
        bool IsUpToDate();
        ISingleSheet<T> Read();

        string Path { get; }
        string Name { get; }
    }

    public class ExcelFile<T> : IExcelFile<T>
        where T : class, new()
    {
        #region members

        private readonly string _typeName;
        private readonly string _path;
        private readonly string _name;
        private readonly ExcelFileConfig _config;
        private readonly ILogFile _log;
        private readonly ISheetReader<T> _reader;

        private DateTime _loadedAt;
        private DateTime _checkedAt;

        #endregion

        #region construction

        public static IExcelFile<T> New(string path, ILogFile log = null, ExcelFileConfig config = null)
        {
            config = config ?? new ExcelFileConfig();
            log = log ?? LogFile.Null;

            return
                new ExcelFile<T>(path, log, config);
        }

        private ExcelFile(string path, ILogFile log, ExcelFileConfig config)
        {
            _typeName = typeof(T).Name;
            _path = path;
            _name = Path.GetFileName(path);
            _config = config;
            _log = log;
            _reader = SheetReader<T>.New(config, log);
            _loadedAt = DateTime.MinValue;
            _checkedAt = DateTime.MinValue;
        }

        #endregion

        #region private

        private ISingleSheet<T> read_first_sheet(string path)
        {
            var file = new FileInfo(path);

            if (!file.Exists)
                throw new ExcelFileException("File '{0}' is not found", path);

                                                                                     // FileShare.ReadWrite - http://stackoverflow.com/a/898017/675116
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var package = new ExcelPackage(stream))
            {
                var book = package.Workbook;

                if (book.Worksheets.Count == 0)
                    throw new ExcelFileException("File '{0}' no sheets found");

                var excel = book.Worksheets[1];
                var data = _reader.ReadSheetData(path, excel);
                var sheet = SingleSheet<T>.New(data, _log);

                return sheet;
            }
        }

        #endregion

        #region interface

        bool IExcelFile<T>.IsUpToDate()
        {
            var now = DateTime.UtcNow;

            var sinceLastCheck = now - _checkedAt;

            if (sinceLastCheck <= _config.MinimalCheckSpan)
                return true;

            var lastUpdatedAt = File.GetLastWriteTimeUtc(_path);
            _checkedAt = now;

            bool isUpToDate = 
                lastUpdatedAt <= _loadedAt;

            return isUpToDate;
        }

        ISingleSheet<T> IExcelFile<T>.Read()
        {
            var now = DateTime.UtcNow;

            var sheet = Try.Catch.Wrap(
                () => read_first_sheet(_path),
                ex => ex.WrapWith<ExcelFileException>("While loading excel data ({0}) from '{1}'", _typeName, _path));

            _loadedAt = now;

            _log.Trace("Excel file '{0}' is loaded and parsed (lines={1} items={2})", _path, sheet.Lines.Length, sheet.Items.Length);

            return sheet;
        }

        string IExcelFile<T>.Path { get { return _path; } }
        string IExcelFile<T>.Name { get { return _name; } }

        #endregion
    }
}