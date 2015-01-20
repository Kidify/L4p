using NUnit.Framework;
using Newtonsoft.Json;

namespace L4p.Common.Json._nunit
{
    [TestFixture]
    class Json_with_adge_cases
    {
        [Test]
        public void empty_object_deseialization()
        {
            var obj = JsonConvert.DeserializeObject("{}");
        }

        [Test]
        public void null_object_ToJson()
        {
            Json_with_adge_cases obj = null;
            var json = obj.ToJson();

            Assert.That(json, Is.EqualTo("null"));
        }
    }
}