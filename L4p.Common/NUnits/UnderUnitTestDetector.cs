using System.Diagnostics;
using System.Linq;

namespace L4p.Common.NUnits
{
    static class UnderUnitTestDetector
    {
        private static readonly string[] NUnitTestMethods = {
            "RunAllTests"
        };

        public static bool RunningUnderUnitTest()
        {
            var stack = new StackTrace();
            var frames = stack.GetFrames();

            if (frames == null)
                return false;

            foreach (var frame in frames)
            {
                var methodName = frame.GetMethod().Name;

                bool isNUnitMethod =
                    NUnitTestMethods.Any(name => name == methodName);

                if (isNUnitMethod)
                    return true;
            }

            return false;
        }
    }
}