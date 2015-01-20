using System;
using System.Threading;

namespace L4p.WebApi
{
    interface IAsyncOperation : IAsyncResult
    {
        void MarkAsSynchronous();
        void NotifyCompletion();
    }

    class AsyncOperation : IAsyncOperation
    {
        #region members

        private readonly AsyncCallback _cb;

        private bool _isCompleted;
        private bool _synchronous;

        #endregion

        #region construction

        public static IAsyncOperation New(AsyncCallback cb)
        {
            return
                new AsyncOperation(cb);
        }

        private AsyncOperation(AsyncCallback cb)
        {
            _cb = cb;
        }

        #endregion

        #region interface

        bool IAsyncResult.IsCompleted { get { return _isCompleted; } }
        WaitHandle IAsyncResult.AsyncWaitHandle { get { return null; } }
        object IAsyncResult.AsyncState { get { return null; } }
        bool IAsyncResult.CompletedSynchronously { get { return _synchronous; } }

        void IAsyncOperation.MarkAsSynchronous()
        {
            _synchronous = true;
        }

        void IAsyncOperation.NotifyCompletion()
        {
            _isCompleted = true;
            _cb(this);
        }

        #endregion
    }
}