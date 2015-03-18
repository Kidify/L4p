using System;
using System.Runtime.CompilerServices;
using L4p.Common.Helpers;
using L4p.Common.Json;

namespace L4p.Common.MsgIsApplications
{
    public interface IMsgIsTheApp
    {
        IMsgContext GetContextFor(object msg);
        void SetContextFor(MsgContext access, IMsgContext context, object msg);     // friend only access
    }

    public class MsgIsTheAppEx : IMsgIsTheApp
    {
        #region members

        private readonly ConditionalWeakTable<object, MsgContextProxy> _contexts;

        #endregion

        #region construction

        public static IMsgIsTheApp New()
        {
            return
                new MsgIsTheAppEx();
        }

        private MsgIsTheAppEx()
        {
            _contexts = new ConditionalWeakTable<object, MsgContextProxy>();
        }

        #endregion

        #region private

        private void validate_it_is_not_a_merge(IMsgContext prev, IMsgContext next, object msg)
        {
            if (prev == null)
                return;

            if (ReferenceEquals(prev, Null.MsgContext))
                return;

            if (ReferenceEquals(prev, next))
                return;

            throw new MsgIsTheAppException(
                "Merging of two applications is not supported; '{0}' {1}", msg.GetType().Name, msg.ToJson());
        }

        #endregion

        #region interface

        IMsgContext IMsgIsTheApp.GetContextFor(object msg)
        {
            var proxy = _contexts.GetOrCreateValue(msg);
            proxy.SetMsgIsTheApp(this);

            proxy.Add(msg);

            return proxy;
        }

        void IMsgIsTheApp.SetContextFor(MsgContext access, IMsgContext context, object msg)
        {
            Validate.NotNull(access);

            MsgContextProxy prev;
            _contexts.TryGetValue(msg, out prev);

            if (prev != null)
            {
                validate_it_is_not_a_merge(prev.Impl, context.Impl, msg);
                return;
            }

            var proxy = new MsgContextProxy(context);
            proxy.SetMsgIsTheApp(this);

            try
            {
                _contexts.Add(msg, proxy);
            }
            catch (ArgumentException)   // key already exists
            {}
        }

        #endregion
    }
}