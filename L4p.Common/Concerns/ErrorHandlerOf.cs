using System;
using System.Linq.Expressions;
using L4p.Common.Extensions;
using L4p.Common.Helpers;
using L4p.Common.Loggers;

namespace L4p.Common.Concerns
{
    public class ErrorHandlerOf<T>
    {
        #region members

        protected delegate Exception MakeException(Exception cause, string msg, params object[] args);

        protected readonly ILogFile _log;
        protected readonly T _impl;
        private readonly MakeException _makeException;

        #endregion

        #region construction

        protected ErrorHandlerOf(ILogFile log, T impl)
        {
            _log = log;
            _impl = impl;
        }

        protected ErrorHandlerOf(ILogFile log, T impl, MakeException makeException)
        {
            _log = log;
            _impl = impl;
            _makeException = makeException;
        }

        #endregion

        #region private

        private string get_method_name(Expression expression)
        {
            Expression body = expression;

            try
            {
                if (body.NodeType == ExpressionType.Lambda)
                {
                    body = ((LambdaExpression) body).Body;
                }

                if (body.NodeType == ExpressionType.Call)
                {
                    var methodCall = (MethodCallExpression) body;

                    return
                        methodCall.Method.Name;
                }

                return
                    "Failed to parse expression tree; expression.NodeType='{0}', body.NodeType='{1}'".Fmt(expression.NodeType, body.NodeType);
            }
            catch
            {
                return "failed_to_get_method_name";
            }
        }

        private Exception new_exception(Exception cause, string msg, params object[] args)
        {
            try
            {
                if (_makeException != null)
                {
                    return
                        _makeException(cause, msg, args);
                }
            }
            catch
            {}

            return
                new ConcernsException(cause, msg, args);
        }

        #endregion

        #region protected

        protected void try_catch_wrap(Expression<Action> methodCall)
        {
            string name = get_method_name(methodCall);

            try
            {
                methodCall.Compile()();
            }
            catch (Exception ex)
            {
                _log.Error(ex);

                throw new_exception(ex,
                    "Unexpected exception in {0}(); {1}", name, ex.Message);
            }
        }

        protected void try_catch_wrap(Expression<Action> methodCall, string msg, params object[] args)
        {
            string name = get_method_name(methodCall);

            try
            {
                methodCall.Compile()();
            }
            catch (Exception ex)
            {
                _log.Error(ex);

                throw new_exception(ex,
                    "Failure in {0}(); {1}", name, msg.Fmt(args));
            }
        }

        protected R try_catch_wrap<R>(Expression<Func<R>> methodCall, string msg, params object[] args)
        {
            string name = get_method_name(methodCall);

            try
            {
                return
                    methodCall.Compile()();
            }
            catch (Exception ex)
            {
                _log.Error(ex);

                throw new_exception(ex,
                    "Failure in {0}(); {1}", name, msg.Fmt(args));
            }
        }

        protected void try_catch_wrap(Expression<Action> methodCall, Func<Exception, Exception> wrapWith)
        {
            string name = get_method_name(methodCall);

            try
            {
                methodCall.Compile()();
            }
            catch (Exception ex)
            {
                _log.Error(ex);

                throw
                    wrapWith(ex);
            }
        }

        protected R try_catch_wrap<R>(Expression<Func<R>> funcCall)
        {
            string name = get_method_name(funcCall);

            try
            {
                return
                    funcCall.Compile()();
            }
            catch (Exception ex)
            {
                _log.Error(ex);

                throw new_exception(ex,
                    "Unexpected exception in {0}(); {1}", name, ex.Message);
            }
        }

        protected R try_catch_wrap<R>(Expression<Func<R>> funcCall, Func<Exception, Exception> wrapWith)
        {
            string name = get_method_name(funcCall);

            try
            {
                return
                    funcCall.Compile()();
            }
            catch (Exception ex)
            {
                _log.Error(ex);

                throw 
                    wrapWith(ex);
            }
        }

        protected void try_catch_handle(Expression<Action> methodCall)
        {
            string name = get_method_name(methodCall);

            try
            {
                methodCall.Compile()();
            }
            catch (Exception ex)
            {
                _log.Error(ex);
            }
        }

        #endregion
    }
}