using System;

namespace L4p.Common.Helpers
{
    public class TrackingId
    {
        public enum Type
        {
            Int,
            ShortStr
        }

        public static string New(Type type = Type.ShortStr)
        {
            var rnd = new Random();

            int p0 = rnd.Next(100);
            int p1 = rnd.Next(100);
            int p2 = rnd.Next(100);

            string trackingId =
                String.Format("{0:00}.{1:00}.{2:00}", p0, p1, p2);

            return trackingId;
        }
    }
}