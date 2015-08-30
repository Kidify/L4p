using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using L4p.Common.DumpToLogs;
using L4p.Common.Extensions;
using L4p.Common.Helpers;
using L4p.Common.PubSub.utils;

namespace L4p.Common.PubSub.client
{
    class Topic
    {
        public string Name { get; set; }
        public Guid Guid { get; set; }
        public Type Type { get; set; }
        public TopicDetails Details { get; set; }
    }

    class HandlerInfo
    {
        public Topic Topic { get; set; }
        public PHandlerAction Call { get; set; }
        public PFilterFunc Filter { get; set; }
        public comm.FilterInfo FilterInfo { get; set; }
    }

    interface ILocalRepo : IHaveDump
    {
        int GetSnapshotId();
        void AddHandler(HandlerInfo handler);
        bool RemoveHandler(HandlerInfo handler);
        HandlerInfo[] GetHandlers(Guid topicGuid, out int snapshotId);
    }

    class LocalRepo : ILocalRepo
    {
        #region members

        private int _snapshotId;
        private readonly LinkedList<HandlerInfo> _handlers;

        #endregion

        #region construction

        public static ILocalRepo New()
        {
            return
                SyncClientRepo.New(
                new LocalRepo());
        }

        private LocalRepo()
        {
            _snapshotId = 1;
            _handlers = new LinkedList<HandlerInfo>();
        }

        #endregion

        #region private

        private void move_shanpshot_id()
        {
            ++_snapshotId;
        }

        #endregion

        #region interface

        int ILocalRepo.GetSnapshotId()
        {
            return _snapshotId;
        }

        void ILocalRepo.AddHandler(HandlerInfo handler)
        {
            Validate.NotNull(handler);
            Validate.NotNull(handler.Topic);
            Validate.NotNull(handler.Call);

            move_shanpshot_id();

            _handlers.AddLast(handler);
        }

        bool ILocalRepo.RemoveHandler(HandlerInfo handler)
        {
            bool wasThere = _handlers.Remove(handler);

            if (wasThere)
                move_shanpshot_id();

            return wasThere;
        }

        HandlerInfo[] ILocalRepo.GetHandlers(Guid topicGuid, out int snapshotId)
        {
            snapshotId = _snapshotId;

            var query =
                from handler in _handlers
                where handler.Topic.Guid == topicGuid
                select handler;

            return
                query.ToArray();
        }

        ExpandoObject IHaveDump.Dump(dynamic root)
        {
            if (root == null)
                root = new ExpandoObject();

            var topics =
                from handler in _handlers
                let topic = handler.Topic
                let hasFilter = handler.Filter != null ? "has filter" : "no filter"
                orderby topic.Name
                select "'{0}' '{1}' ({2}) '{3}'".Fmt(
                    topic.Name, hasFilter, topic.Type.FullName, topic.Guid);

            root.SnapshotId = _snapshotId;
            root.Count = _handlers.Count;
            root.Topics = topics.ToArray();

            return root;
        }

        #endregion
    }

    class SyncClientRepo : ILocalRepo
    {
        private readonly object _mutex = new object();
        private readonly ILocalRepo _impl;

        public static ILocalRepo New(ILocalRepo impl) { return new SyncClientRepo(impl); }
        private SyncClientRepo(ILocalRepo impl) { _impl = impl; }

        int ILocalRepo.GetSnapshotId() { lock (_mutex) return _impl.GetSnapshotId(); }
        void ILocalRepo.AddHandler(HandlerInfo handler) { lock (_mutex) _impl.AddHandler(handler); }
        bool ILocalRepo.RemoveHandler(HandlerInfo handler) { lock (_mutex) return _impl.RemoveHandler(handler); }
        HandlerInfo[] ILocalRepo.GetHandlers(Guid topicGuid, out int snapshotId) { lock (_mutex) return _impl.GetHandlers(topicGuid, out snapshotId); }
        ExpandoObject IHaveDump.Dump(dynamic root) { lock(_mutex) return _impl.Dump(); }
    }
}