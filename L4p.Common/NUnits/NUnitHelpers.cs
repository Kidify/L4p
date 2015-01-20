using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace L4p.Common.NUnits
{
    public static class NUnitHelpers
    {
        private static bool _runningUnderUnitTest;

        static NUnitHelpers()
        {
            _runningUnderUnitTest = UnderUnitTestDetector.RunningUnderUnitTest();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string PathOf(string file)
        {
            string sourcePath = new StackTrace(true).GetFrame(1).GetFileName();

            string sourceFolder = Path.GetDirectoryName(sourcePath);
            string path = Path.Combine(sourceFolder, file);

            return path;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string AsFullPath(this string file)
        {
            string sourcePath = new StackTrace(true).GetFrame(1).GetFileName();

            string sourceFolder = Path.GetDirectoryName(sourcePath);
            string path = Path.Combine(sourceFolder, file);

            return path;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string ReadText(this string file)
        {
            string sourcePath = new StackTrace(true).GetFrame(1).GetFileName();

            string sourceFolder = Path.GetDirectoryName(sourcePath);
            string path = Path.Combine(sourceFolder, file);

            var text = File.ReadAllText(path);

            return text;
        }

        public static bool RunningUnderUnitTest
        {
            get { return _runningUnderUnitTest; }
        }
    }
}