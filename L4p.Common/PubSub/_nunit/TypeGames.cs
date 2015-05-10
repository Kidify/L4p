using System;
using NUnit.Framework;

namespace L4p.Common.PubSub._nunit
{
    [TestFixture, Explicit]
    class TypeGames
    {
        // experimental point of view: Type.GUID is calculated based on full class name

        class TestIt
        {
            public string Str { get; set; }
            public int Int { get; set; }
        }
        class Other
        {
            public string Str { get; set; }
        }

        [Test]
        public void get_type_id()
        {
            Console.WriteLine("{0}: {1}", typeof(TestIt).Name, typeof(TestIt).GUID);
            Console.WriteLine("{0}: {1}", typeof(Other).Name, typeof(Other).GUID);
        }
    }
}