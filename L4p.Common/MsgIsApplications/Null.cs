using System;
using System.Dynamic;
using L4p.Common.DumpToLogs;

namespace L4p.Common.MsgIsApplications
{
    class NullMsgIsTheApp : IMsgIsTheApp
    {
        IMsgContext IMsgIsTheApp.GetContextFor(object msg) { return Null.MsgContext; }
        void IMsgIsTheApp.SetContextFor(MsgContext access, IMsgContext context, object msg) {}
    }

    class NullMsgContext : IMsgContext
    {
        ExpandoObject IHaveDump.Dump(dynamic root) { return root; }
        IMsgContext IMsgContext.Impl { get { return null; } }
        IMsgContext IMsgContext.Lock { set {} }
        T IMsgContext.Add<T>(T value) { return value; }
        T IMsgContext.Get<T>() { return default(T); }
        T IMsgContext.Remove<T>() { return default(T); }
        T IMsgContext.Replace<T>(T value) { return value; }
        void IMsgContext.SetMsgIsTheApp(MsgIsTheAppEx msgIsTheApp) {}
    }

    static class Null
    {
        public readonly static IMsgIsTheApp MsgIsTheApp = new NullMsgIsTheApp();
        public readonly static IMsgContext MsgContext = new NullMsgContext();
    }
}