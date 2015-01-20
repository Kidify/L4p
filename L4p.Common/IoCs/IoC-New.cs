using System;
using Moq;

namespace L4p.Common.IoCs
{
    public partial class IoC
    {
        #region helpers

        private static void validate_these_are_not_mocks(params object[] dependencies)
        {
            Type mockType = typeof(Mock<>);

            for (int indx = 0; indx < dependencies.Length; indx++)
            {
                Type type = dependencies[indx].GetType();

                bool isMock =
                    type.IsGenericType &&
                    type.GetGenericTypeDefinition() == mockType;

                if (isMock == false)
                    continue;

                Type mockedType = type.GetGenericArguments()[0];

                throw new IocResolverException(
                    "Parameter at index {0} is a mock of type 'Mock<{1}>' (mocks with not-mocks are mixed?)", indx, mockedType.Name);
            }
        }

        #endregion

        #region with dependencies

        public static IIoC New<T1>(T1 d1)
            where T1 : class
        {
            validate_these_are_not_mocks(d1);

            var ioc = new IoC();

            ioc.Setup()
                .Map<T1>().To(d1);

            return ioc;
        }

        public static IIoC New<T1, T2>(T1 d1, T2 d2)
            where T1 : class
            where T2 : class
        {
            validate_these_are_not_mocks(d1, d2);

            var ioc = new IoC();

            ioc.Setup()
                .Map<T1>().To(d1)
                .Map<T2>().To(d2);

            return ioc;
        }

        public static IIoC New<T1, T2, T3>(T1 d1, T2 d2, T3 d3)
            where T1 : class
            where T2 : class
            where T3 : class
        {
            validate_these_are_not_mocks(d1, d2, d3);

            var ioc = new IoC();

            ioc.Setup()
                .Map<T1>().To(d1)
                .Map<T2>().To(d2)
                .Map<T3>().To(d3);

            return ioc;
        }

        public static IIoC New<T1, T2, T3, T4>(T1 d1, T2 d2, T3 d3, T4 d4)
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
        {
            validate_these_are_not_mocks(d1, d2, d3, d4);

            var ioc = new IoC();

            ioc.Setup()
                .Map<T1>().To(d1)
                .Map<T2>().To(d2)
                .Map<T3>().To(d3)
                .Map<T4>().To(d4);

            return ioc;
        }

        public static IIoC New<T1, T2, T3, T4, T5>(T1 d1, T2 d2, T3 d3, T4 d4, T5 d5)
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
        {
            validate_these_are_not_mocks(d1, d2, d3, d4, d5);

            var ioc = new IoC();

            ioc.Setup()
                .Map<T1>().To(d1)
                .Map<T2>().To(d2)
                .Map<T3>().To(d3)
                .Map<T4>().To(d4)
                .Map<T5>().To(d5);

            return ioc;
        }

        public static IIoC New<T1, T2, T3, T4, T5, T6>(T1 d1, T2 d2, T3 d3, T4 d4, T5 d5, T6 d6)
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
        {
            validate_these_are_not_mocks(d1, d2, d3, d4, d5, d6);

            var ioc = new IoC();

            ioc.Setup()
                .Map<T1>().To(d1)
                .Map<T2>().To(d2)
                .Map<T3>().To(d3)
                .Map<T4>().To(d4)
                .Map<T5>().To(d5)
                .Map<T6>().To(d6);

            return ioc;
        }

        public static IIoC New<T1, T2, T3, T4, T5, T6, T7>(T1 d1, T2 d2, T3 d3, T4 d4, T5 d5, T6 d6, T7 d7)
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
        {
            validate_these_are_not_mocks(d1, d2, d3, d4, d5, d6, d7);

            var ioc = new IoC();

            ioc.Setup()
                .Map<T1>().To(d1)
                .Map<T2>().To(d2)
                .Map<T3>().To(d3)
                .Map<T4>().To(d4)
                .Map<T5>().To(d5)
                .Map<T6>().To(d6)
                .Map<T7>().To(d7);

            return ioc;
        }

        #endregion

        #region with mocks

        public static IIoC New<T1>(Mock<T1> m1) 
            where T1 : class 
        { return New(m1.Object); }

        public static IIoC New<T1, T2>(Mock<T1> m1, Mock<T2> m2)
            where T1 : class
            where T2 : class
        { return New(m1.Object, m2.Object); }

        public static IIoC New<T1, T2, T3>(Mock<T1> m1, Mock<T2> m2, Mock<T3> m3)
            where T1 : class
            where T2 : class
            where T3 : class
        { return New(m1.Object, m2.Object, m3.Object); }

        public static IIoC New<T1, T2, T3, T4>(Mock<T1> m1, Mock<T2> m2, Mock<T3> m3, Mock<T4> m4)
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
        { return New(m1.Object, m2.Object, m3.Object, m4.Object); }

        public static IIoC New<T1, T2, T3, T4, T5>(Mock<T1> m1, Mock<T2> m2, Mock<T3> m3, Mock<T4> m4, Mock<T5> m5)
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
        { return New(m1.Object, m2.Object, m3.Object, m4.Object, m5.Object); }

        public static IIoC New<T1, T2, T3, T4, T5, T6>(Mock<T1> m1, Mock<T2> m2, Mock<T3> m3, Mock<T4> m4, Mock<T5> m5, Mock<T6> m6)
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
        { return New(m1.Object, m2.Object, m3.Object, m4.Object, m5.Object, m6.Object); }

        public static IIoC New<T1, T2, T3, T4, T5, T6, T7>(Mock<T1> m1, Mock<T2> m2, Mock<T3> m3, Mock<T4> m4, Mock<T5> m5, Mock<T6> m6, Mock<T7> m7)
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
        { return New(m1.Object, m2.Object, m3.Object, m4.Object, m5.Object, m6.Object, m7.Object); }

        #endregion
    }
}