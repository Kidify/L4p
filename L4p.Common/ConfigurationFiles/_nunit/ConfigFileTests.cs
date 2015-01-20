using System;
using System.IO;
using NUnit.Framework;
using L4p.Common.NUnits;

namespace L4p.Common.ConfigurationFiles._nunit
{
    [TestFixture]
    class Using_FileIo
    {
        [Test]
        public void Read_timestamp_of_non_existing_file()
        {
            var notExistingAt = DateTime.Parse("1601-01-01 00:00:00.000");

            {
                var lastUpdatedAt = File.GetLastWriteTimeUtc("d:/tmp/a_non_exisitng_file.js");
                Assert.That(lastUpdatedAt, Is.EqualTo(notExistingAt));
            }

            {
                var lastUpdatedAt = File.GetLastWriteTimeUtc("a:/tmp/a_non_exisitng_file.js");
                Assert.That(lastUpdatedAt, Is.EqualTo(notExistingAt));
            }
        }
    }

    [TestFixture]
    class When_configuration_file_is_loaded
    {
        class AConfig
        {
            public string Item { get; set; }
            public string WithComment { get; set; }
            public string WithBlockComment { get; set; }
        }

        [Test]
        public void It_should_find_the_coresponding_key()
        {
            {
                var path = "multiple_configurations_aaa.js".AsFullPath();
                var json = File.ReadAllText(path);

                var parser = Json2Config<AConfig>.New(path);
                var config = parser.ParseJson(path, json);

                Assert.That(config.Item, Is.EqualTo("AAA"));
            }

            {
                var path = "multiple_configurations_bbb.js".AsFullPath();
                var json = File.ReadAllText(path);

                var parser = Json2Config<AConfig>.New(path);
                var config = parser.ParseJson(path, json);

                Assert.That(config.Item, Is.EqualTo("BBB"));
            }

            {
                var path = "multiple_configurations_ccc.js".AsFullPath();
                var json = File.ReadAllText(path);

                var parser = Json2Config<AConfig>.New(path);
                var config = parser.ParseJson(path, json);

                Assert.That(config.Item, Is.EqualTo("CCC"));
            }
        }

        [Test]
        public void It_should_load_single_config()
        {
            var path = "single_configuration.js".AsFullPath();
            var json = File.ReadAllText(path);

            var parser = Json2Config<AConfig>.New(path);
            var config = parser.ParseJson(path, json);

            Assert.That(config.Item, Is.EqualTo("Single"));
            Assert.That(config.WithComment, Is.EqualTo("\\"));
            Assert.That(config.WithBlockComment, Is.EqualTo("/*"));
        }
    }
}