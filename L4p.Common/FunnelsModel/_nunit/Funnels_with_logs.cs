using NUnit.Framework;
using L4p.Common.Extensions;
using L4p.Common.FunnelsModel.hub;
using L4p.Common.FunnelsModel.hub.wcf;

namespace L4p.Common.FunnelsModel._nunit
{
    [TestFixture, Explicit]
    class Funnels_with_logs
    {
        [Test]
        public void Issue_log_messages()
        {
            var funnel = Funnel.GetBy("Unit/Test/Funnels_with_logs");

            var data = new {Value = 123};
            var data2 = new {Value = 0};

            funnel.Publish(data);
            data2 = funnel.Get(data2);

            Assert.That(data2.Value, Is.EqualTo(123));

            for (int indx = 0; indx < 10; indx++)
            {
                funnel.Log.Info.Issue_log_messages("It is a log message number {0}", indx);
            }

            Funnels.Stop();

            Assert.Pass();
        }
    }
}