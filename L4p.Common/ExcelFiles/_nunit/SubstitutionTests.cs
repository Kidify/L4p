using System;
using System.Collections.Generic;
using NUnit.Framework;
using L4p.Common.Loggers;

namespace L4p.Common.ExcelFiles._nunit
{
    [TestFixture]
    class SubstitutionTests
    {
        [Test]
        public void substitute_columns()
        {
            var data = new SheetData<RuleSample> {
                Properties = new [] {"OpCode", "Prop", "Prop2"},
                Lines = new [] {
                    new SheetLine {RowNo = 1, Values = new [] {"11", "12"}},
                    new SheetLine {RowNo = 1, Values = new [] {"21", "22"}}
                },
                ColumnName2ItsIndx = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase) {
                    {"Prop", 1},
                    {"Prop2", 2},
                }
            };

            var sheet = SingleSheet<RuleSample>.New(data, LogFile.Console);

            Assert.That(sheet.MapColumnsToValues(1, "It should be {Prop} here"), Is.EqualTo("It should be 11 here"));
            Assert.That(sheet.MapColumnsToValues(1, "It should be {Prop2} here"), Is.EqualTo("It should be 12 here"));
            Assert.That(sheet.MapColumnsToValues(1, "It should be {Prop} {prop2} here"), Is.EqualTo("It should be 11 12 here"));
            Assert.That(sheet.MapColumnsToValues(1, "It should be {Prop} {prop} {prop2} {prop2} here"), Is.EqualTo("It should be 11 11 12 12 here"));

            Assert.That(sheet.MapColumnsToValues(2, "It should be {Prop} here"), Is.EqualTo("It should be 21 here"));
            Assert.That(sheet.MapColumnsToValues(2, "It should be {Prop2} here"), Is.EqualTo("It should be 22 here"));
            Assert.That(sheet.MapColumnsToValues(2, "It should be {prop2} {prop} here"), Is.EqualTo("It should be 22 21 here"));

            Assert.That(sheet.MapColumnsToValues(1, "It should be {Prop} {prop3} here"), Is.EqualTo("It should be 11 {prop3} here"));
        }
    }
}