using NUnit.Framework;

namespace L4p.Common.FunnelsModel._nunit
{
    [TestFixture]
    class When_Funnel_is_used
    {
        [Test]
        public void It_should_just_work()
        {
            var ss = new SomeStruct {Id = 1234, Comment = "my comment"};

            var funnel = Funnel.GetBy(MyTag.Tag1);
            funnel.Publish(ss);

            var funnel2 = Funnel.GetBy(MyTag.Tag1);
            var ss2 = funnel2.Get<SomeStruct>();

            Assert.That(ss2, Is.Not.SameAs(ss));
            Assert.That(ss2.Id, Is.EqualTo(ss.Id));
            Assert.That(ss2.Comment, Is.EqualTo(ss.Comment));

            var ss3 = funnel2.Get<SomeStruct>();
            Assert.That(ss3, Is.Not.SameAs(ss2));
        }

        [Test]
        public void Data_is_immutable()
        {
            var ss = new SomeStruct { Id = 1234, Comment = "my comment" };

            var funnel = Funnel.GetBy(MyTag.Tag1);
            funnel.Publish(ss);

            ss.Id = 4321;

            var funnel2 = Funnel.GetBy(MyTag.Tag1);
            var ss2 = funnel2.Get<SomeStruct>();

            Assert.That(ss2.Id, Is.EqualTo(1234));
        }

        [Test]
        public void Publish_is_data_distribution()
        {
            {
                var ss = new SomeStruct { Id = 1234, Comment = "my comment" };

                var funnel = Funnel.GetBy(MyTag.Tag1);
                funnel.Publish(ss);
            }

            {
                var funnel = Funnel.GetBy(MyTag.Tag1);
                var ss = funnel.Get<SomeStruct>();

                Assert.That(ss.Id, Is.EqualTo(1234));
            }
        }
    }
}