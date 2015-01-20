using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using L4p.Common.Extensions;
using L4p.Common.Json;

namespace L4p.Common.PubSub.utils._nunit
{
    class SomeMsg
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    [TestFixture]
    class PlayTests
    {
        private delegate bool PolymorphicFilter(object closure, object msg);

        static PolymorphicFilter build_filter_func(MethodInfo method)
        {
            var closure = Expression.Parameter(typeof(object));
            var closureType = method.DeclaringType;

            var msg = Expression.Parameter(typeof(object));
            var msgType = method.GetParameters()[0].ParameterType;

            Expression<PolymorphicFilter> expr =
                Expression.Lambda<PolymorphicFilter>(
                    Expression.Call(
                        Expression.Convert(closure, closureType),
                        method,
                        Expression.Convert(msg, msgType)),
                    closure,
                    msg);

            return expr.Compile();
        }

        bool execute_filter(SomeMsg msg, Func<SomeMsg, bool> filter)
        {
            var moduleName = filter.Method.Module.Name;
            var methodToken = filter.Method.MetadataToken;
            var targetJson = filter.Target.AsSingleLineJson();

            //-----

            var query =
                from a in AppDomain.CurrentDomain.GetAssemblies()
                let m = a.GetModules()[0]
                where m.Name == moduleName
                select m;

            var module = query.FirstOrDefault();

            var method = (MethodInfo) module.ResolveMethod(methodToken);
            var targetType = method.DeclaringType;

            var target = targetJson.Parse(targetType);

            var filter2 = build_filter_func(method);
            var result = filter2(target, msg);
//            var result = (bool) method.Invoke(target, new object[] {msg});

            return result;
        }

        [Test]
        public void expressions()
        {
            var msgId = 123;
            var name = "msg";

            var result = execute_filter(new SomeMsg {Id = 123, Name = "msg"},
                msg => msg.Id == msgId && msg.Name == name);

            Assert.That(result, Is.True);
        }
    }
}
