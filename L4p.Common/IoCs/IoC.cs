using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace L4p.Common.IoCs
{
    public partial class IoC : IIoC
    {
        #region members

        private readonly Dictionary<Type, object> _instances;
        private readonly Dictionary<Type, Func<object>> _factories;

        #endregion

        #region construction

        public static IIoC New()
        {
            return 
                new IoC();
        }

        private IoC()
        {
            _instances = new Dictionary<Type, object>();
            _factories = new Dictionary<Type, Func<object>>();
        }

        #endregion

        #region private

        private void validate_obj_implements_interface<T>(object instance)
            where T : class
        {
            if (instance as T != null)
                return;

            throw new IocResolverException(
                "Object '{0}' of type '{1}' does not implement the '{2}' interface", instance, instance.GetType().Name, typeof(T).Name);
        }

        private void validate_instance_can_be_registred(Type type, object instance)
        {
            Trace.Assert(instance != null);

            object prevInstance;

            bool instanceIsAlreadyHere = _instances.TryGetValue(type, out prevInstance);
            bool factoryIsAlreadyHere = _factories.ContainsKey(type);

            bool firstRegistration = !instanceIsAlreadyHere && !factoryIsAlreadyHere;

            if (firstRegistration)
                return;

            bool sameInstance = ReferenceEquals(instance, prevInstance);

            if (sameInstance)
                return;

            if (factoryIsAlreadyHere)
            {
                throw new IocResolverException(
                    "A factory is already registered for type '{0}'; (instance='{1}')", type.FullName, instance);
            }

            throw new IocResolverException(
                "A previous instance '{0}' is already registered for type '{1}'; (instance='{2}')", prevInstance, type.FullName, instance);
        }

        private void validate_factory_can_be_registred(Type type)
        {
            object prevInstance;

            bool instanceIsAlreadyHere = _instances.TryGetValue(type, out prevInstance);
            bool factoryIsAlreadyHere = _factories.ContainsKey(type);

            bool firstRegistration = !instanceIsAlreadyHere && !factoryIsAlreadyHere;

            if (firstRegistration)
                return;

            if (factoryIsAlreadyHere)
            {
                throw new IocResolverException(
                    "A factory is already registered for type '{0}'", type.FullName);
            }

            throw new IocResolverException(
                "A previous instance '{0}' is already registered for type '{1}'", prevInstance, type.FullName);
        }

        private T resolve<T>() where T : class
        {
            Type type = typeof(T);

            object instance;

            if (_instances.TryGetValue(type, out instance))
            {
                return (T) instance;
            }

            Func<object> factory;

            if (_factories.TryGetValue(type, out factory))
            {
                object obj = factory();
                validate_obj_implements_interface<T>(obj);

                return (T) obj;
            }

            return null;
        }

        #endregion

        #region IIocResolver

        IIoC IIoC.RegisterInstance<T>(object instance)
        {
            Type type = typeof (T);

            if (instance == null)
            {
                throw new IocResolverException(
                    "Can't register a null instance; type='{0}'", type.FullName);
            }

            validate_obj_implements_interface<T>(instance);
            validate_instance_can_be_registred(type, instance);
            _instances[type] = instance;

            return this;
        }

        IIoC IIoC.RegisterFactory<T>(Func<object> factory)
        {
            Type type = typeof(T);

            if (factory == null)
            {
                throw new IocResolverException(
                    "Can't register a null factory; type='{0}'", type.FullName);
            }

            validate_factory_can_be_registred(type);
            _factories[type] = factory;

            return this;
        }

        IIoC IIoC.RegisterLazy<T>(Func<T> lazy)
        {
            throw new NotImplementedException();
        }

        T IIoC.Resolve<T>()
        {
            Type type = typeof(T);

            var obj = resolve<T>();

            if (obj != null)
                return obj;

            throw
                new IocResolverException(String.Format("Can't resolve '{0}'; implementation was not registered", type.Name));
        }

        T IIoC.Resolve<T>(Func<T> factory)
        {
            var obj = resolve<T>();

            if (obj != null)
                return obj;

            obj = factory();

            return obj;
        }

        T IIoC.Resolve<T>(Func<IIoC, T> factory)
        {
            var obj = resolve<T>();

            if (obj != null)
                return obj;

            obj = factory(this);

            return obj;
        }

        T IIoC.SingleInstance<T>(Func<T> factory)
        {
            Type type = typeof(T);

            var instance = resolve<T>() ?? factory();
            _instances[type] = instance;

            return instance;
        }

        T IIoC.SingleInstance<T>(Func<IIoC, T> factory)
        {
            Type type = typeof(T);

            var instance = resolve<T>() ?? factory(this);
            _instances[type] = instance;

            return instance;
        }

        void IIoC.Resolve<T>(Action<T> whenRegistered)
        {
            throw new NotImplementedException();
        }

        bool IIoC.HasImplementationFor<T>()
        {
            Type type = typeof(T);

            bool instanceIsHere = _instances.ContainsKey(type);
            bool factoryIsHere = _factories.ContainsKey(type);

            return
                instanceIsHere || factoryIsHere;
        }

        #endregion
    }
}