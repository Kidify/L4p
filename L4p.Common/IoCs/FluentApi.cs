using System;
using Moq;

namespace L4p.Common.IoCs
{
    public static class FluentApi
    {
        #region api

        public interface ISetupMapping
        {
            IMapTo<I> Map<I>() where I : class;
            ISetupMapping Mock<I>() where I : class;
            IIfMissing<I> IfMissing<I>() where I : class;
            ISetupMapping Register<T>(T impl) where T : class;
            ISetupMapping Register<T>(Func<T> lazy) where T : class;
        }

        public interface IMapTo<I> where I : class
        {
            ISetupMapping To<T>(T instance) where T : I;
            ISetupMapping To<T>(Func<T> factory) where T : I;
        }

        public interface IIfMissing<I> where I : class
        {
            ISetupMapping MapTo(I instance);
            ISetupMapping MockIt();
        }

        #endregion

        #region implementation

        public static ISetupMapping Setup(this IIoC ioc)
        {
            return
                new SetupMapping(ioc);
        }

        class SetupMapping : ISetupMapping
        {
            private readonly IIoC _ioc;

            public SetupMapping(IIoC ioc)
            {
                _ioc = ioc;
            }

            IMapTo<I> ISetupMapping.Map<I>()
            {
                return
                    new MapTo<I>(_ioc);
            }

            ISetupMapping ISetupMapping.Mock<I>()
            {
                var mock = new Mock<I>();
                _ioc.RegisterInstance<I>(mock.Object);
                return this;
            }

            IIfMissing<I> ISetupMapping.IfMissing<I>()
            {
                return
                    new MockIt<I>(_ioc);
            }

            ISetupMapping ISetupMapping.Register<T>(T impl)
            {
                _ioc.RegisterInstance<T>(impl);
                return this;
            }

            ISetupMapping ISetupMapping.Register<T>(Func<T> lazy)
            {
                try
                {
                    var impl = lazy();
                    _ioc.RegisterInstance<T>(impl);
                }
                catch (IocResolverException)
                {
                    _ioc.RegisterLazy(lazy);
                }

                return this;
            }
        }

        class MapTo<I> : IMapTo<I>
            where I : class
        {
            private readonly IIoC _ioc;

            public MapTo(IIoC ioc)
            {
                _ioc = ioc;
            }

            ISetupMapping IMapTo<I>.To<T>(T instance)
            {
                _ioc.RegisterInstance<I>(instance);

                return 
                    _ioc.Setup();
            }

            ISetupMapping IMapTo<I>.To<T>(Func<T> factory)
            {
                _ioc.RegisterFactory<I>(() => factory());

                return
                    _ioc.Setup();
            }
        }

        class MockIt<I> : IIfMissing<I>
            where I : class
        {
            private readonly IIoC _ioc;

            public MockIt(IIoC ioc)
            {
                _ioc = ioc;
            }

            ISetupMapping IIfMissing<I>.MapTo(I instance)
            {
                bool isThere = _ioc.HasImplementationFor<I>();

                if (!isThere)
                {
                    _ioc.RegisterInstance<I>(instance);
                }

                return
                    _ioc.Setup();
            }

            ISetupMapping IIfMissing<I>.MockIt()
            {
                bool isThere = _ioc.HasImplementationFor<I>();

                if (!isThere)
                {
                    var mock = new Mock<I>();
                    _ioc.RegisterInstance<I>(mock.Object);
                }

                return 
                    _ioc.Setup();
            }
        }

        #endregion
    }
}