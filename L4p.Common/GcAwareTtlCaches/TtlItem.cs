using System;
using System.Threading;

namespace L4p.Common.GcAwareTtlCaches
{
    interface ITtlItem<TInstance>
        where TInstance : class
    {
        TInstance Instance { get; }
        DateTime DeadSince { get; }
    }

    class TtlItem<TInstance> : ITtlItem<TInstance>, IReferenceCounter
        where TInstance : class
    {
        #region members

        private readonly TInstance _instance;

        private int _referenceCounter;
        private DateTime _deadSince;

        #endregion

        #region construction

        public static ITtlItem<TInstance> New(TInstance instance)
        {
            return
                new TtlItem<TInstance>(instance);
        }

        private TtlItem(TInstance instance)
        {
            _instance = instance;

            _referenceCounter = 0;
            _deadSince = DateTime.MaxValue;
        }

        #endregion

        #region ITtlItem

        public TInstance Instance { get { return _instance; } }
        public DateTime DeadSince { get { return _deadSince; } }

        #endregion

        #region IReferenceCounter

        void IReferenceCounter.LinkInstance()
        {
            int count = Interlocked.Increment(ref _referenceCounter);

            if (count == 1)
                _deadSince = DateTime.MaxValue;
        }

        void IReferenceCounter.ReleaseInstance()
        {
            int count = Interlocked.Decrement(ref _referenceCounter);

            if (count == 0)
                _deadSince = DateTime.UtcNow;
        }

        #endregion
    }
}