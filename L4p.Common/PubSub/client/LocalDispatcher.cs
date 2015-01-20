using System;
using System.Dynamic;
using System.Threading;
using L4p.Common.DumpToLogs;
using L4p.Common.Extensions;
using L4p.Common.Helpers;
using L4p.Common.IoCs;
using L4p.Common.Loggers;

namespace L4p.Common.PubSub.client
{
    interface ILocalDispatcher : IHaveDump
    {
        void DispatchLocalMsg(Topic topic, object msg);
        bool DispatchRemoteMsg(object msg, HandlerInfo[] handlers);
    }

    class LocalDispatcher : ILocalDispatcher
    {
        #region counters

        class Counters
        {
            public int LocalMsgs;
            public int LocalMsgsDispatched;
            public int RemoteMsgs;
            public int RemoteMsgsDispatched;
            public int HandlersCalled;
            public int HandlersFailed;
            public int HandlersFiltered;
            public int FiltersFailed;
        }

        #endregion

        #region members

        private readonly Counters _counters;
        private readonly ILogFile _log;
        private readonly ILocalRepo _lrepo;

        #endregion

        #region construction

        public static ILocalDispatcher New(IIoC ioc)
        {
            return
                new LocalDispatcher(ioc);
        }

        private LocalDispatcher(IIoC ioc)
        {
            _counters = new Counters();
            _log = ioc.Resolve<ILogFile>();
            _lrepo = ioc.Resolve<ILocalRepo>();
        }

        #endregion

        #region private

        private bool msg_should_be_filtered(HandlerInfo handler, object msg)
        {
            if (handler.Filter == null)
                return false;

            var shouldBeFiltered = false;

            try
            {
                var filterIsMatched = handler.Filter(msg);
                shouldBeFiltered = !filterIsMatched;
            }
            catch
            {
                Interlocked.Increment(ref _counters.FiltersFailed);
            }

            return shouldBeFiltered;
        }

        private void dispatch_msg(HandlerInfo handler, object msg)
        {
            try
            {
                handler.Call(msg);
                Interlocked.Increment(ref _counters.HandlersCalled);
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Failed to dispatch local message; topic='{0}'", handler.Topic.Name);
                Interlocked.Increment(ref _counters.HandlersFailed);
            }
        }

        private bool dispatch_to_handlers(object msg, HandlerInfo[] handlers)
        {
            if (handlers.IsEmpty())
                return false;

            var hasListeners = false;

            foreach (var handler in handlers)
            {
                if (msg_should_be_filtered(handler, msg))
                {
                    Interlocked.Increment(ref _counters.HandlersFiltered);
                    continue;
                }

                hasListeners = true;
                dispatch_msg(handler, msg);
            }

            return hasListeners;
        }

        #endregion

        #region interface

        void ILocalDispatcher.DispatchLocalMsg(Topic topic, object msg)
        {
            int snapshotId;
            var handlers = _lrepo.GetHandlers(topic.Guid, out snapshotId);

            var hasListeners = dispatch_to_handlers(msg, handlers);
            Interlocked.Increment(ref _counters.LocalMsgs);

            if (hasListeners)
                Interlocked.Increment(ref _counters.LocalMsgsDispatched);
        }

        bool ILocalDispatcher.DispatchRemoteMsg(object msg, HandlerInfo[] handlers)
        {
            var hasListeners = dispatch_to_handlers(msg, handlers);
            Interlocked.Increment(ref _counters.RemoteMsgs);

            if (hasListeners)
                Interlocked.Increment(ref _counters.RemoteMsgsDispatched);

            return hasListeners;
        }

        ExpandoObject IHaveDump.Dump(dynamic root)
        {
            if (root == null)
                root = new ExpandoObject();

            root.Counters = _counters;

            return root;
        }

        #endregion
    }
}