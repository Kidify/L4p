using NUnit.Framework;
using L4p.Common.Loggers;
using L4p.Common.NUnits;

namespace L4p.Common.ExcelFiles._nunit
{
    class RuleSample
    {
        public string OpCode { get; set; }
        public int RuleId { get; set; }
        public int Priority { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public decimal UpgradeTo { get; set; }
        public int[] BusinessUnits { get; set; }
        public int[] UserGroups { get; set; }
        public int[] Processors { get; set; }
        public string EnglishMsg { get; set; }
        public string ArabicMsg { get; set; }
        public string RussianMsg { get; set; }
    }

    [TestFixture]
    class ExcelFileTests
    {
        [Test]
        public void Read_deposit_rules_excel_sample()
        {
            var path = NUnitHelpers.PathOf("deposit-rules.xlsx");
            var file = ExcelFile<RuleSample>.New(path, LogFile.Console);

            var sheet = file.Read();

            Assert.That(sheet.RowCount, Is.GreaterThan(4));
            Assert.That(sheet.ColCount, Is.EqualTo(13));
        }
    }
}