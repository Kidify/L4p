using System.Collections.Generic;

namespace L4p.Common.Extensions
{
    public static class ArrayExtensions
    {
        public static bool IsEmpty<T>(this T[] array)
        {
            if (array == null)
                return true;

            if (array.Length == 0)
                return true;

            return false;
        }

        public static bool IsNotEmpty<T>(this T[] array)
        {
            return
                !array.IsEmpty();
        }

        public static bool IsEmpty<T>(this Queue<T> array)
        {
            if (array == null)
                return true;

            if (array.Count == 0)
                return true;

            return false;
        }

        public static bool IsNotEmpty<T>(this Queue<T>[] array)
        {
            return
                !array.IsEmpty();
        }
    }
}