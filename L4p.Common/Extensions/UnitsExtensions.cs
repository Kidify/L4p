namespace L4p.Common.Extensions
{
    public static class UnitsExtensions
    {
        public static int Megabytes(this int bytes)
        {
            return
                bytes * 1024 * 1024;
        }

        public static int Megabytes(this double bytes)
        {
            return
                (int)(bytes + .5) * 1024 * 1024;
        }

        public static int Kilobytes(this int bytes)
        {
            return
                bytes * 1024;
        }

        public static int Kilobytes(this double bytes)
        {
            return
                (int)(bytes + .5) * 1024;
        }
    }
}