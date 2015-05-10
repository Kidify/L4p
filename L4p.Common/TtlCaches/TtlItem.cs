using System;
using System.Threading;

namespace L4p.Common.TtlCaches
{
    interface ITtlItem<TBody>
        where TBody : class
    {
        TBody Body { get; }
        DateTime DeadSince { get; }
    }

    class TtlItem<TBody> : ITtlItem<TBody>, IReferenceCounter
        where TBody : class
    {
        #region members

        private readonly TBody _body;

        private int _referenceCounter;
        private DateTime _deadSince;

        #endregion

        #region construction

        public static ITtlItem<TBody> New(TBody body)
        {
            return
                new TtlItem<TBody>(body);
        }

        private TtlItem(TBody body)
        {
            _body = body;

            _referenceCounter = 0;
            _deadSince = DateTime.MaxValue;
        }

        #endregion

        #region ITtlItem

        public TBody Body { get { return _body; } }
        public DateTime DeadSince { get { return _deadSince; } }

        #endregion

        #region IReferenceCounter

        void IReferenceCounter.LinkBody()
        {
            int count = Interlocked.Increment(ref _referenceCounter);

            if (count == 1)
                _deadSince = DateTime.MaxValue;
        }

        void IReferenceCounter.ReleaseBody()
        {
            int count = Interlocked.Decrement(ref _referenceCounter);

            if (count == 0)
                _deadSince = DateTime.UtcNow;
        }

        #endregion
    }
}