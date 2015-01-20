using System;
using NUnit.Framework;

namespace L4p.Common.NUnits
{
    public static class AssertIt
    {
        public static void Fails(TestDelegate action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            Assert.That(() => {}, Throws.Exception);
        }

        public static void Succeeds(TestDelegate action)
        {
            Assert.That(action, Throws.Nothing);
        }
    }
}