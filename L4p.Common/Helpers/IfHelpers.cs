using System;

namespace L4p.Common.Helpers
{
    public static class If
    {
        public static void Else(bool ifElse, Action ifAction, Action elseAction)
        {
            if (ifElse)
                ifAction();
            else
                elseAction();
        }

        public static T Else<T>(bool ifElse, Func<T> ifFunc, Func<T> elseFunc)
        {
            if (ifElse)
                return ifFunc();

            return elseFunc();
        }
    }
}