using System;
using System.Threading;
using L4p.Common.ForeverThreads;
using L4p.Common.FunnelsModel.config;
using L4p.Common.IoCs;
using L4p.Common.Loggers;

namespace L4p.Common.FunnelsModel.io
{
    interface IIoThreads
    {
        void Start();
        void Stop();
    }

    class IoThreads : IIoThreads
    {
        #region members

        private readonly ILogFile _log;
        private readonly IFunnelsManagerEx _funnels;
        private readonly IFmConfigRa _config;
        private readonly IForeverThread[] _pool; 

        #endregion

        #region construction

        public static IIoThreads New(IIoC ioc)
        {
            return
                new IoThreads(ioc);
        }

        private IForeverThread[] create_threads(int count)
        {
            var threads = new IForeverThread[count];

            for (int indx = 0; indx < count; indx++)
            {
                IForeverThread thr = null;
                thr = ForeverThread.New(() => thread_loop(thr), _log);        // access to modified closure: use it to initialize thread loop

                threads[indx] = thr;
            }

            return threads;
        }

        private IoThreads(IIoC ioc)
        {
            _log = ioc.Resolve<ILogFile>();
            _funnels = ioc.Resolve<IFunnelsManagerEx>();
            _config = ioc.Resolve<IFmConfigRa>();
            _pool = create_threads(_config.Config.Client.SinkThreadsCount);
        }

        #endregion

        #region private

        private bool process_io()
        {
            try
            {
                bool mightHaveMoreWork = _funnels.ProcessIo();
                return mightHaveMoreWork;
            }
            catch (Exception ex)
            {
                _log.Warn(ex, "Failed to process io");
            }

            // exception means that there might have been other io to process
            return true;        
        }

        private void thread_loop(IForeverThread thr)
        {
            _log.Trace("IoThread is started");

            while (true)
            {
                if (thr.StopRequestIsPosted())
                    break;

                bool mightHaveMoreWork = process_io();

                if (mightHaveMoreWork)
                    continue;

                Thread.Sleep(1);
            }

            _log.Trace("IoThread is done");
        }

        #endregion

        #region interface

        void IIoThreads.Start()
        {
            foreach (var thr in _pool)
            {
                thr.Start();
            }
        }

        void IIoThreads.Stop()
        {
            foreach (var thr in _pool)
            {
                thr.PostStopRequest();
            }

            foreach (var thr in _pool)
            {
                thr.Stop();
            }
        }

        #endregion
    }
}