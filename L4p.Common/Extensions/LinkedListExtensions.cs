using System;
using System.Collections.Generic;
using L4p.Common.Helpers;

namespace L4p.Common.Extensions
{
    public static class LinkedListExtensions
    {
        public static IEnumerable<T> Reverse<T>(this LinkedList<T> list)
        {
            var it = list.Last;
            while (it != null)
            {
                yield return it.Value;
                it = it.Previous;
            }
        }

        public static T FindLast<T>(this LinkedList<T> list, Func<T, bool> math)
            where T : class
        {
            var it = list.Last;

            do
            {
                if (it == null)
                    return null;

                var elem = it.Value;
                Validate.NotNull(elem);

                if (math(elem))
                    return elem;

                it = it.Previous;
            }
            while (true);
        }

        public static int RemoveAll<T>(this LinkedList<T> list, Predicate<T> match)
        {
            if (list == null)
                return 0;

            if (list.Count == 0)
                return 0;

            var count = 0;
            var it = list.First;

            while (it != null)
            {
                var next = it.Next;

                if (match(it.Value))
                {
                    list.Remove(it);
                    count++;
                }

                it = next;
            }

            return count;
        }

        public static bool IsEmpty<T>(this LinkedList<T> list)
        {
            if (list == null)
                return true;

            if (list.Count == 0)
                return true;

            return false;
        }

        public static bool IsNotEmpty<T>(this LinkedList<T> list)
        {
            return
                !list.IsEmpty();
        }
    }
}