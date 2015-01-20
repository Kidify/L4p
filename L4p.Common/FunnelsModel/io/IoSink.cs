using System;
using L4p.Common.ActionQueues;
using L4p.Common.FunnelsModel.comm;
using L4p.Common.IoCs;
using L4p.Common.Loggers;

namespace L4p.Common.FunnelsModel.io
{
    interface IIoRepo
    {}

    interface IIoSink
    {
        void PostIo(Ioop ioop);
        bool ProcessIo();
    }

    class IoSink : IIoSink
    {
        #region Counters

        class Counters
        {
            public int OpPosted { get; set; }
            public int OpSent { get; set; }
            public int OpCompleted { get; set; }
            public int OpFailed { get; set; }
        }

        #endregion

        #region members

        private readonly ILogFile _log;
        private readonly IIoQueue _que;
        private readonly Counters _counters;

        #endregion

        #region construction

        public static IIoSink New(IIoC ioc)
        {
            return
                new IoSink(ioc);
        }

        private IoSink(IIoC ioc)
        {
            _log = ioc.Resolve<ILogFile>();
            _que = ioc.Resolve<IIoQueue>();
            _counters = new Counters();
        }

        #endregion

        #region private

        private void accumulate_ioop_counters(Ioop ioop)
        {
            lock (_counters)
            {
                _counters.OpPosted += ioop.Counters.Posted;
                _counters.OpSent += ioop.Counters.Sent;
                _counters.OpCompleted += ioop.Counters.Completed;
                _counters.OpFailed += ioop.Counters.Failed;
            }
        }

        private void post_io(Ioop ioop, DateTime now)
        {
            ioop.Events.PostedAt = now;
            ioop.Counters.Posted++;

            ioop.Next = Todo.Send;
            _que.Push(ioop);
        }

        private void send_io(Ioop ioop, DateTime now)
        {
            ioop.Events.SentAt = now;
            ioop.Counters.Sent++;

            ioop.Next = Todo.Complete;
            ioop.OnIoComplete = () => dispatch_ioop(ioop);
            ioop.OnIoError = ex => handle_io_error(ioop, ex);
            ioop.Services.Out.Send(ioop);
        }

        private void complete_io(Ioop ioop, DateTime now)
        {
            ioop.Counters.Completed++;
            _que.Complete(ioop);

            accumulate_ioop_counters(ioop);
        }

        private void handle_io_error(Ioop ioop, Exception ex)
        {
            ioop.Counters.Failed++;
            ioop.IoError = ex;

            _log.Warn(ex, "Failure in ioop {0}", ioop.SequenceId);

            ioop.Services.Out.CloseConnection();
        }

        private void dispatch_ioop(Ioop ioop)
        {
            var now = DateTime.Now;

            switch (ioop.Next)
            {
                case Todo.Post:
                    post_io(ioop, now);
                    break;

                case Todo.Send:
                    send_io(ioop, now);
                    break;

                case Todo.Complete:
                    complete_io(ioop, now);
                    break;
            }
        }

        #endregion

        #region interface

        void IIoSink.PostIo(Ioop ioop)
        {
            ioop.Next = Todo.Post;
            dispatch_ioop(ioop);

            // validate queue counters (issue warnings)
        }

        bool IIoSink.ProcessIo()
        {
            var ioop = _que.Get(Todo.Send);

            if (ioop == null)
                return false;

            try
            {
                dispatch_ioop(ioop);
            }
            finally
            {
                _que.Release(ioop);
            }

            return true;
        }

        #endregion
    }

}