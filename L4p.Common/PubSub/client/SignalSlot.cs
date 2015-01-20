using System;

namespace L4p.Common.PubSub.client
{
    public interface ISignalSlot : IDisposable
    {
        void Cancel();
    }

    class SignalSlot : ISignalSlot
    {
        #region members

        private readonly ISignalsManagerEx _signals;
        private readonly HandlerInfo _handler;

        private bool _canceled;

        #endregion

        #region construction

        public static ISignalSlot New(ISignalsManagerEx signals, HandlerInfo handler)
        {
            return
                new SignalSlot(signals, handler);
        }

        private SignalSlot(ISignalsManagerEx signals, HandlerInfo handler)
        {
            _signals = signals;
            _handler = handler;
        }

        #endregion

        #region private

        private void cancel()
        {
            if (_canceled)
                return;

            _canceled = true;

            _signals.CancelSubscription(this, _handler);
        }

        #endregion

        #region interface

        void IDisposable.Dispose()
        {
            try
            {
                cancel();
            }
            catch
            {}
        }

        void ISignalSlot.Cancel()
        {
            cancel();
        }

        #endregion
    }
}