using System;
using System.Threading;

namespace L4p.Common.Concerns
{
    class ManyReadersSingleWriter<T>
    {
        #region members

        private readonly ReaderWriterLockSlim _lock;
        protected readonly T _impl;

        #endregion

        #region construction

        protected ManyReadersSingleWriter(T impl)
        {
            _lock = new ReaderWriterLockSlim();
            _impl = impl;
        }

        #endregion

        #region private

        protected void using_read_lock(Action action)
        {
            _lock.EnterReadLock();

            try
            {
                action();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        protected R using_read_lock<R>(Func<R> func)
        {
            _lock.EnterReadLock();

            try
            {
                return func();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        protected void using_write_lock(Action action)
        {
            _lock.EnterWriteLock();

            try
            {
                action();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        protected R using_write_lock<R>(Func<R> func)
        {
            _lock.EnterWriteLock();

            try
            {
                return
                    func();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        #endregion
    }
}