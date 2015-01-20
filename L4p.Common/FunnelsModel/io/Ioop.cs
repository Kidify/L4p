using System;

namespace L4p.Common.FunnelsModel.io
{
    interface IIoFactory
    {
        IIoReader NewShopReader(string uri);
        IIoWriter NewShopWriter(string uri);
    }

    enum Todo { Post, Send, Complete, Done }

    class Ioop
    {
        public int SequenceId { get; set; }

        public Todo Next { get; set; }

        public Action<comm.IFunnelsShop> Io { get; set; }
        public Action OnIoComplete { get; set; }
        public Action<Exception> OnIoError { get; set; }
        public Exception IoError { get; set; }

        public IoopBehaviour Behaviour { get; set; }
        public IoopEvents Events { get; set; }
        public IoopCounters Counters { get; set; }
        public IoopServices Services { get; set; }
        public IoopState State { get; set; }

        public Ioop()
        {
            Behaviour = new IoopBehaviour();
            Events = new IoopEvents();
            Counters = new IoopCounters();
            Services = new IoopServices();
            State = new IoopState();
        }
    }

    class IoopBehaviour
    {
    }

    class IoopEvents
    {
        public DateTime PostedAt { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime CompletedAt { get; set; }
    }

    class IoopCounters
    {
        public int Posted { get; set; }
        public int Sent { get; set; }
        public int Completed { get; set; }
        public int Failed { get; set; }
    }

    class IoopServices
    {
        public IIoQueue Que { get; set; }
        public IIoWriter Out { get; set; }
        public IIoReader In { get; set; }
        public IIoFactory Factory { get; set; }
    }

    class IoopState
    {
        public object QueLock { get; set; }
        public Exception SentError { get; set; }
    }
}