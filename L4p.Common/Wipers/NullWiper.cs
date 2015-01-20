using System;

namespace L4p.Common.Wipers
{
    public class NullWiper : IWiper
    {
        event Action IWiper.que
        {
            add { }
            remove { throw new ShouldNotBeCalledException(); }
        }

        void IWiper.Proceed() {}
    }

    public static class NullLogFileWrapper
    {
        public static IWiper WrapIfNull(this IWiper wiper)
        {
            return
                wiper ?? Wiper.Null;
        }
    }
}