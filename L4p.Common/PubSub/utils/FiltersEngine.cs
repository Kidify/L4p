using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Newtonsoft.Json;
using L4p.Common.DumpToLogs;
using L4p.Common.Extensions;
using L4p.Common.Helpers;
using L4p.Common.Json;
using L4p.Common.Loggers;
using L4p.Common.PubSub.client;

namespace L4p.Common.PubSub.utils
{
    delegate void PHandlerAction(object msg);
    delegate bool PFilterFunc(object msg);

    interface IFiltersEngine : IHaveDump
    {
        PFilterFunc[] BuildFilters(comm.FilterInfo[] infos);
        bool FilterCanBeTransferred<T>(Func<T, bool> filter);
        comm.FilterInfo MakeFilterInfo<T>(Func<T, bool> filter, StackFrame filterAt = null);
        comm.TopicFilterMsg BuildFilterTopicMsg(string self, int snapshotId, comm.PublishMsg pmsg, HandlerInfo[] handlers, TopicDetails topicDetails);
    }

    class FiltersEngine : IFiltersEngine
    {
        #region counters

        class Counters
        {
            public int HandlerWithotFilter;
            public int FilterBuildFailed;
        }

        #endregion

        #region members

        private delegate bool StaticFilterFunc(object msg);
        private delegate bool InstanceFilterFunc(object context, object msg);

        private readonly Counters _counters;
        private readonly ILogFile _log;

        #endregion

        #region construction

        public static IFiltersEngine New(ILogFile log = null)
        {
            return
                new FiltersEngine(log.WrapIfNull());
        }

        private FiltersEngine(ILogFile log)
        {
            _counters = new Counters();
            _log = log;
        }

        #endregion

        #region private

        private static PFilterFunc build_static_filter(MethodInfo method)
        {
            var msg = Expression.Parameter(typeof(object));
            var msgType = method.GetParameters()[0].ParameterType;

            Expression<StaticFilterFunc> expr =
                Expression.Lambda<StaticFilterFunc>(
                    Expression.Call(
                        null,
                        method,
                        Expression.Convert(msg, msgType)),
                    msg);

            var lfilter = expr.Compile();

            return
                x => lfilter(x);
        }

        private static PFilterFunc build_instance_filter(MethodInfo method, object target)
        {
            var closure = Expression.Parameter(typeof(object));
            var closureType = method.DeclaringType;

            var msg = Expression.Parameter(typeof(object));
            var msgType = method.GetParameters()[0].ParameterType;

            Expression<InstanceFilterFunc> expr =
                Expression.Lambda<InstanceFilterFunc>(
                    Expression.Call(
                        Expression.Convert(closure, closureType),
                        method,
                        Expression.Convert(msg, msgType)),
                    closure,
                    msg);

            var lfilter = expr.Compile();

            return
                x => lfilter(target, x);
        }

        private object create_target(Type type, string json)
        {
            var target = Activator.CreateInstance(type);

            try
            {
                JsonConvert.PopulateObject(json, target);
            }
            catch
            {
                _log.Warn("Failed to populate target of type '{0}' from '{1}'; target={2}", type.Name, json, target);
            }

            return target;
        }

        private PFilterFunc build_pfilter_from(comm.FilterInfo info)
        {
            var moduleName = info.ModuleName.ToLowerInvariant();

            var query =
                from a in AppDomain.CurrentDomain.GetAssemblies()
                let m = a.GetModules()[0]
                where m.Name.ToLowerInvariant() == moduleName
                select m;

            var module = query.FirstOrDefault();

            if (module == null)
                throw new SignalsException("Module '{0}' is not found in current application domain", info.ModuleName);

            var method = (MethodInfo) module.ResolveMethod(info.MethodToken);

            if (method == null)
                throw new SignalsException("Module '{0}': can't resolve methodToken={1}", info.ModuleName, info.MethodToken);

            var filterHasNoContext = method.IsStatic;

            PFilterFunc pfilter;

            if (filterHasNoContext)
            {
                pfilter = build_static_filter(method);
            }
            else
            {
                var targetType = method.DeclaringType;
                var target = create_target(targetType, info.ContextAsJson);
                pfilter = build_instance_filter(method, target);
            }

            return pfilter;
        }

        private bool filter_can_be_transferred<T>(Func<T, bool> filter)
        {
            var target = filter.Target;

            if (target == null)
                return true;

            if (target.GetType().Name.StartsWith("<>"))
                return true;

            return false;
        }

        private string serialize_target(object target, bool isTransferable)
        {
            if (isTransferable == false)
                return null;

            if (target == null)
                return null;

            return
                target.AsSingleLineJson();
        }

        private string get_source_info(StackFrame filterAt)
        {
            var filePath = filterAt.GetFileName();

            if (filePath.IsEmpty())
                return "n/a";

            var fileName = Path.GetFileName(filePath);
            var lineNo = filterAt.GetFileLineNumber();

            if (lineNo == 0)
                return "n/a";

            return
                "{0}.{1}".Fmt(fileName, lineNo);
        }

        #endregion

        #region interface

        PFilterFunc[] IFiltersEngine.BuildFilters(comm.FilterInfo[] infos)
        {
            if (infos.IsEmpty())
                return null;

            var filters = new PFilterFunc[infos.Length];
            bool allFiltersAreBuilt = true;

            for (int indx = 0; indx < infos.Length; indx++)
            {
                var info = infos[indx];

                try
                {
                    var filter = build_pfilter_from(info);
                    filters[indx] = filter;
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref _counters.FilterBuildFailed);
                    allFiltersAreBuilt = false;
                    _log.Warn("Failed to build filter ({0}) for {1}", ex.Message, info.ToJson());
                }
            }

            if (allFiltersAreBuilt == false)
                return null;

            return filters;
        }

        bool IFiltersEngine.FilterCanBeTransferred<T>(Func<T, bool> filter)
        {
            return
                filter_can_be_transferred(filter);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        comm.FilterInfo IFiltersEngine.MakeFilterInfo<T>(Func<T, bool> filter, StackFrame filterAt)
        {
            if (filterAt == null)
                filterAt = new StackFrame(1);

            if (filter == null)
                return null;

            var moduleName = filter.Method.Module.Name;
            var methodToken = filter.Method.MetadataToken;
            var definedAt = get_source_info(filterAt);
            var target = filter.Target;
            var isTransferable = filter_can_be_transferred(filter);

            var contextAsJson = serialize_target(target, isTransferable);

            var info = new comm.FilterInfo
                {
                    IsTransferable = isTransferable,
                    ModuleName = moduleName,
                    MethodToken = methodToken,
                    DefinedAt = definedAt,
                    ContextAsJson = contextAsJson
                };

            return info;
        }

        comm.TopicFilterMsg IFiltersEngine.BuildFilterTopicMsg(string self, int snapshotId, comm.PublishMsg pmsg, HandlerInfo[] handlers, TopicDetails topicDetails)
        {
            // if we are here then the pmsg was not consumed by any handler

            var filterMsg = new comm.TopicFilterMsg
                {
                    AgentToFilter = self,
                    SnapshotId = snapshotId,
                    TopicGuid = pmsg.TopicGuid,
                    TopicName = pmsg.TopicName,
                };

            if (handlers.IsEmpty())
                return filterMsg;           // filter the whole topic

            var list = new List<comm.FilterInfo>();

            foreach (var handler in handlers)
            {
                if (handler.Filter == null)     // should no happen since such a handler would have consumed the message
                {
                    Interlocked.Increment(ref _counters.HandlerWithotFilter);
                    continue;
                }

                Validate.NotNull(handler.FilterInfo);
                list.Add(handler.FilterInfo);
            }

            var filters = list.Count > 0 ?
                list.ToArray() : null;

            filterMsg.Filters = filters;

            return filterMsg;
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