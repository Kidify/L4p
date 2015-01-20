using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace L4p.Common.PubSub.client
{
    public interface ISignalsSlot
    {
        ISignalsSlot SubscribeTo<T>(Action<T> callback, Func<T, bool> filter = null, StackFrame filterAt = null)
            where T : class, new();
        ISignalsSlot Add(ISignalSlot slot);
        void Cancel();
    }

    public class SignalsSlot : ISignalsSlot
    {
        #region members

        private readonly object _mutex;
        private readonly List<ISignalSlot> _slots;
        private readonly ISignalsManager _signals;

        #endregion

        #region construction

        public static ISignalsSlot New(ISignalsManager signals = null)
        {
            return
                new SignalsSlot(signals);
        }

        private SignalsSlot(ISignalsManager signals)
        {
            _mutex = new object();
            _slots = new List<ISignalSlot>();
            _signals = signals;
        }

        #endregion

        #region private

        private void add_slot(ISignalSlot slot)
        {
            lock (_mutex)
            {
                _slots.Add(slot);
            }
        }

        #endregion

        #region interface

        [MethodImpl(MethodImplOptions.NoInlining)]
        ISignalsSlot ISignalsSlot.SubscribeTo<T>(Action<T> callback, Func<T, bool> filter, StackFrame filterAt)
        {
            if (filterAt == null)
                filterAt = new StackFrame(1);

            var slot = _signals.SubscribeTo<T>(callback, filter, filterAt);
            add_slot(slot);

            return this;
        }

        ISignalsSlot ISignalsSlot.Add(ISignalSlot slot)
        {
            add_slot(slot);
            return this;
        }

        void ISignalsSlot.Cancel()
        {
            ISignalSlot[] slots;

            lock (_mutex)
            {
                slots = _slots.ToArray();
                _slots.Clear();
            }

            foreach (var slot in slots)
            {
                slot.Cancel();
            }
        }

        #endregion
    }
}