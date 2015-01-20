using System;
using NUnit.Framework;

namespace L4p.Common.Helpers.nunit
{
    [TestFixture]
    class TrackingIdTests
    {
        [Test]
        public void leading_zeros()
        {
            Assert.That(String.Format("{0:00}-{1:00}-{2:00}", 0, 1, 2), Is.EqualTo("00-01-02"));
        }
    }
}