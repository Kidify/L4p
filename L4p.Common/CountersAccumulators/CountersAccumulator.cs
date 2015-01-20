using System;
using System.Collections.Generic;
using System.Linq;
using L4p.Common.Helpers;
using L4p.Common.Loggers;

namespace L4p.Common.CountersAccumulators
{
    public interface IHaveCounters
    {
        void AccumulateCounters(ICountersAccumulator accumulator);
    }

    public interface ICountersAccumulator
    {
        ICountersAccumulator Sum<T>(T counters) where T : class, new();
        ICountersAccumulator Average<T>(T counters) where T : class, new();
        ICountersAccumulator StDev<T>(T counters) where T : class, new();
        T GetSum<T>() where T : class, new();
        T GetAverage<T>() where T : class, new();
        T GetStDev<T>() where T : class, new();
        T GetDiffFrom<T>(T from) where T : class, new();

        ICountersAccumulator Visit(IHaveCounters container);
    }

    public class CountersAccumulator : ICountersAccumulator
    {
        #region members

        private static readonly object SystemAssembly = typeof(string).Assembly;

        private readonly ILogFile _log;
        private readonly object _mutex;
        private readonly Dictionary<Type, object> _sums;

        #endregion

        #region construction

        public static ICountersAccumulator New(ILogFile log)
        {
            return
                new CountersAccumulator(log);
        }

        private CountersAccumulator(ILogFile log)
        {
            _log = log;
            _mutex = new Object();
            _sums = new Dictionary<Type, object>();
        }

        #endregion

        #region private

        private T get_sums<T>() 
            where T : class, new()
        {
            Type type = typeof(T);
            object mine;

            lock (_mutex)
            {
                if (_sums.TryGetValue(type, out mine))
                {
                    return (T) mine;
                }

                mine = new T();
                _sums.Add(type, mine);

                return (T) mine;
            }
        }

        private void accumulate_int_properites(Type type, object counters, object mine)
        {
            var props =
                from
                    prop in type.GetProperties()
                where
                    prop.PropertyType == typeof(int)
                select
                    prop;

            foreach (var prop in props)
            {
                int value =
                    (int) prop.GetValue(counters, null) + (int) prop.GetValue(mine, null);

                prop.SetValue(mine, value, null);
            }
        }

        private void accumulate_int_fields(Type type, object counters, object mine)
        {
            var fields =
                from
                    field in type.GetFields()
                where
                    field.FieldType == typeof(int)
                select
                    field;

            foreach (var field in fields)
            {
                int value =
                    (int) field.GetValue(counters) + (int) field.GetValue(mine);

                field.SetValue(mine, value);
            }
        }

        private void accumulate_double_properties(Type type, object counters, object mine)
        {
            var props =
                from
                    prop in type.GetProperties()
                where
                    prop.PropertyType == typeof(double)
                select
                    prop;

            foreach (var prop in props)
            {
                double value =
                    (double) prop.GetValue(counters, null) + (double) prop.GetValue(mine, null);

                prop.SetValue(mine, value, null);
            }
        }

        private void accumulate_inner_counters(Type type, object counters, object mine)
        {


            var props =
                from
                    prop in type.GetProperties()
                where
                    prop.PropertyType.IsClass &&
                    !ReferenceEquals(prop.PropertyType.Assembly, SystemAssembly)
                select
                    prop;

            foreach (var prop in props)
            {
                object inner = prop.GetValue(counters, null);
                object myInner = prop.GetValue(mine, null);

                if (inner == null)
                    continue;

                if (myInner == null)
                {
                    myInner = Activator.CreateInstance(inner.GetType());
                    prop.SetValue(mine, myInner, null);
                }

                accumulate_counters(inner.GetType(), inner, myInner);
            }
        }

        private void accumulate_counters(Type type, object counters, object mine)
        {
            accumulate_int_properites(type, counters, mine);
            accumulate_int_fields(type, counters, mine);
            accumulate_double_properties(type, counters, mine);
            accumulate_inner_counters(type, counters, mine);
        }

        private void accumulate_sum<T>(T counters, T mine)
        {
            Type type = typeof(T);
            accumulate_counters(type, counters, mine);
        }

        #endregion

        #region 
        #endregion

        ICountersAccumulator ICountersAccumulator.Sum<T>(T counters)
        {
            T mine = get_sums<T>();

            lock (mine)
            {
                accumulate_sum(counters, mine);
            }

            return this;
        }

        ICountersAccumulator ICountersAccumulator.Average<T>(T counters)
        {
            throw new NotImplementedException();
        }

        ICountersAccumulator ICountersAccumulator.StDev<T>(T counters)
        {
            throw new NotImplementedException();
        }

        T ICountersAccumulator.GetSum<T>()
        {
            T mine = get_sums<T>();
            T result = new T();

            accumulate_sum(mine, result);

            return result;
        }

        T ICountersAccumulator.GetAverage<T>()
        {
            throw new NotImplementedException();
        }

        T ICountersAccumulator.GetStDev<T>()
        {
            throw new NotImplementedException();
        }

        T ICountersAccumulator.GetDiffFrom<T>(T from)
        {
            throw new NotImplementedException();
        }

        ICountersAccumulator ICountersAccumulator.Visit(IHaveCounters container)
        {
            Try.Catch.Handle(
                () => container.AccumulateCounters(this),
                ex => _log.Error("Failed to accumulate counters; container='{0}'; {1}", container.GetType().Name, ex.Message));

            return this;
        }
    }
}