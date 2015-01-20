using System;
using System.Diagnostics;
using System.Dynamic;
using L4p.Common.DumpToLogs;
using L4p.Common.PubSub.contexts;

namespace L4p.Common.PubSub.client
{
    class NotInitializedSignals : ISignalsManager
    {
        private static readonly string NotInitializedMsg = "Signals are not initialized (or failed to initialize)";

        #region members
        #endregion

        #region construction

        public static ISignalsManager New()
        {
            return
                new NotInitializedSignals();
        }

        private NotInitializedSignals()
        {
        }

        #endregion

        #region private
        #endregion

        #region interface

        ExpandoObject IHaveDump.Dump(dynamic root)
        {
            if (root == null)
                root = new ExpandoObject();

            root.Error = NotInitializedMsg;

            return root;
        }

        ISignalSlot ISignalsManager.SubscribeTo<T>(Action<T> callback, Func<T, bool> filter, StackFrame filterAt)
        {
            throw 
                new SignalsException(NotInitializedMsg);
        }

        void ISignalsManager.Publish<T>(T msg)
        {
            throw
                new SignalsException(NotInitializedMsg);
        }

        ISessionContext ISignalsManager.Context
        {
            get
            {
                throw new SignalsException(NotInitializedMsg);
            }
        }

        void ISignalsManager.StartAgent()
        {
            throw
                new SignalsException(NotInitializedMsg);
        }

        void ISignalsManager.StopAgent()
        {
            throw
                new SignalsException(NotInitializedMsg);
        }

        #endregion
    }
}