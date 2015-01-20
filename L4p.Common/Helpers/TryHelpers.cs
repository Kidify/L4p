using System;
using L4p.Common.Loggers;

namespace L4p.Common.Helpers
{
    public static class Try
    {
        public static class Catch
        {
            public static void Rethrow(Action action, Action onError)
            {
                try
                {
                    action();
                }
                catch
                {
                    onError();
                    throw;
                }
            }

            public static void Rethrow(Action action, Action<Exception> onError)
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    onError(ex);
                    throw;
                }
            }

            public static void Rethrow(Action action, Func<Exception, object> onError)
                // Have to cast down to object since resolving Func with dynamic is ambiguous at runtime
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    object wrapped = onError(ex);
                    var outerException = wrapped as Exception;

                    if (outerException != null)
                    {
                        throw outerException;
                    }

                    throw;
                }
            }

            public static R Rethrow<R>(Func<R> func, Action onError)
            {
                try
                {
                    return func();
                }
                catch
                {
                    onError();
                    throw;
                }
            }

            public static R Rethrow<R>(Func<R> func, Action<Exception> onError)
            {
                try
                {
                    return func();
                }
                catch (Exception ex)
                {
                    onError(ex);
                    throw;
                }
            }

            public static R Rethrow<R>(Func<R> func, Func<Exception, object> onError)
            {
                try
                {
                    return func();
                }
                catch (Exception ex)
                {
                    object wrapped = onError(ex);
                    var outerException = wrapped as Exception;

                    if (outerException != null)
                    {
                        throw outerException;
                    }

                    throw;
                }
            }

            public static void Handle(Action action, Action onError)
            {
                try
                {
                    action();
                }
                catch
                {
                    onError();
                }
            }

            public static R Handle<R>(R @default, Func<R> func, Action onError)
            {
                try
                {
                    return
                        func();
                }
                catch
                {
                    onError();
                    return @default;
                }
            }

            public static R Handle<R>(R @default, Func<R> func)
            {
                try
                {
                    return
                        func();
                }
                catch
                {
                    return @default;
                }
            }

            public static void Handle(Action action, Action<Exception> onError)
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    onError(ex);
                }
            }

            public static R Handle<R>(R @default, Func<R> func, Action<Exception> onError)
            {
                try
                {
                    return
                        func();
                }
                catch (Exception ex)
                {
                    onError(ex);
                    return @default;
                }
            }

            public static void Wrap(Action action, Func<Exception, Exception> wrapper)
            {
                try
                {
                    action();
                }
                catch (Exception inner)
                {
                    throw wrapper(inner);
                }
            }

            public static R Wrap<R>(Func<R> func, Func<Exception, Exception> wrapper)
            {
                try
                {
                    return
                        func();
                }
                catch (Exception inner)
                {
                    throw 
                        wrapper(inner);
                }
            }

            public static void Wrap(ILogFile log, Action action, Func<Exception, Exception> wrapper)
            {
                try
                {
                    action();
                }
                catch (Exception inner)
                {
                    var ex = wrapper(inner);
                    log.Error(ex.Message);
                    throw ex;
                }
            }

            public static R Wrap<R>(ILogFile log, Func<R> func, Func<Exception, Exception> wrapper)
            {
                try
                {
                    return
                        func();
                }
                catch (Exception inner)
                {
                    var ex = wrapper(inner);
                    log.Error(ex.Message);
                    throw ex;
                }
            }
        }
    }
}