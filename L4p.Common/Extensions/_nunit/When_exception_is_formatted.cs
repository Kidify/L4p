using System;
using NUnit.Framework;

namespace L4p.Common.Extensions._nunit
{
    class LevelOneException : Exception { public LevelOneException(string msg, Exception inner) : base(msg, inner) { } }
    class LevelTwoException : Exception { public LevelTwoException(string msg, Exception inner) : base(msg, inner) { } }
    class LevelThreeException : Exception { public LevelThreeException(string msg, Exception inner) : base(msg, inner) { } }

    [TestFixture]
    class When_exception_hirarych_is_formatted
    {
        class WithExceptionData
        {
            public string LevelMsg { get; set; }
            public int Count { get; set; }
        }

        private void level_three()
        {
            try
            {
                throw new NullReferenceException("testing a null reference");
            }
            catch (Exception ex)
            {
                throw ex.WrapWith<LevelThreeException>("Exception on level three '{0}'", "null pointer?");
            }
        }

        private void level_two()
        {
            try
            {
                level_three();
            }
            catch (Exception ex)
            {
                var data = new {Specific = "Data from level two", Count = 123};
                throw ex.WrapWith<LevelTwoException>("Exception on level two '{0}'", "testing testing testing")
                        .SetData(data);
            }
        }

        private void level_one()
        {
            try
            {
                level_two();
            }
            catch (Exception ex)
            {
                var data = new WithExceptionData {LevelMsg = "Data from level one", Count = 321 };
                throw ex.WrapWith<LevelOneException>("Exception on level one '{0}'", "testing testing testing")
                        .SetData(data);
            }
        }

        [Test]
        public void It_should_contain_all_inner_exceptions_and_data()
        {
            try
            {
                level_one();
            }
            catch (Exception ex)
            {
                string str = ex.FormatHierarchy();
                Console.WriteLine(str);
            }
            
            Assert.Pass();
        }
    }

    [TestFixture]
    class When_exception_verbose_is_formatted
    {
        public void not_implemented()
        {
            Assert.Fail();
        }
    }
}