using System;
using System.Diagnostics;
using System.Dynamic;
using System.Threading;
using L4p.Common.DumpToLogs;
using L4p.Common.IoCs;
using L4p.Common.Loggers;
using L4p.Common.PubSub.utils;

namespace L4p.Common.PubSub.client
{
    interface ILocalFactory : IHaveDump
    {
        Topic make_topic(Type type);
        HandlerInfo make_handler<T>(Topic topic, Action<T> call, Func<T, bool> filter, StackFrame filterAt);
    }

    class LocalFactory : ILocalFactory
    {
        #region counters

        class Counters
        {
            public int NonTransferableFilters;
        }

        #endregion

        #region members

        private readonly Counters _counters;
        private readonly ILogFile _log;
        private readonly IFiltersEngine _filters;
        private readonly ITopicsRepo _topics;

        #endregion

        #region construction

        public static ILocalFactory New(IIoC ioc)
        {
            return
                new LocalFactory(ioc);
        }

        private LocalFactory(IIoC ioc)
        {
            _counters = new Counters();
            _log = ioc.Resolve<ILogFile>();
            _filters = ioc.Resolve<IFiltersEngine>();
            _topics = ioc.Resolve<ITopicsRepo>();
        }

        #endregion

        private PHandlerAction make_phandler<T>(Action<T> handler)
        {
            return
                msg => handler((T)msg);
        }

        private PFilterFunc make_pfilter<T>(Func<T, bool> filter)
        {
            if (filter == null)
                return null;

            return
                msg => filter((T) msg);
        }

        #region private

        #endregion

        #region interface

        Topic ILocalFactory.make_topic(Type type)
        {
            var topicName = type.Name;
            var topicGuid = type.GUID;
            var details = _topics.GetTopicDetails(topicGuid, topicName);

            var topic = new Topic
                {
                    Name = topicName,
                    Guid = topicGuid,
                    Type = type,
                    Details = details
                };

            return topic;
        }

        HandlerInfo ILocalFactory.make_handler<T>(Topic topic, Action<T> call, Func<T, bool> filter, StackFrame filterAt)
        {
            var filterInfo = _filters.MakeFilterInfo(filter, filterAt);

            if (filterInfo != null)
            if (filterInfo.IsTransferable == false)
            {
                Interlocked.Increment(ref _counters.NonTransferableFilters);
                _log.Warn("Topic '{0}': filter '{1}' ({2}) can't be transferred (using member variables in lambda?)", topic.Name, filter.Method.Name);
            }

            var handler = new HandlerInfo
                {
                    Topic = topic,
                    Call = make_phandler(call),
                    Filter = make_pfilter(filter),
                    FilterInfo = filterInfo
                };

            return handler;
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