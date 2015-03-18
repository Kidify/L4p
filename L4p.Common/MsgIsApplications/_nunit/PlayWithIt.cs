using L4p.Common.NUnits;
using NUnit.Framework;

namespace L4p.Common.MsgIsApplications._nunit
{
    [TestFixture]
    class PlayWithIt
    {
        class OneMsg {}
        class SecondMsg {}
        class OtherMsg {}

        [Test]
        public void basic_interface_play()
        {
            var msg = new OneMsg();
            var second = new SecondMsg();

            msg.MsgIsTheApp().Add(second);
            msg.MsgIsTheApp().Add(second);      // it is idempotent

            var second2 = msg.MsgIsTheApp().Get<SecondMsg>();
            var msg2 = second2.MsgIsTheApp().Get<OneMsg>();

            Assert.AreSame(second, second2);
            Assert.AreSame(msg, msg2);
        }

        [Test]
        public void When_merging_two_application_It_should_throw()
        {
            var msg = new OneMsg();
            var second = new SecondMsg();
            msg.MsgIsTheApp().Add(second);

            var msg2 = new OneMsg();
            var other = new OtherMsg();
            msg2.MsgIsTheApp().Add(other);

            AssertIt.Fails(() => msg.MsgIsTheApp().Add(other));
        }
    }
}