using System;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using L4p.Common.Extensions;
using L4p.Common.Loggers;

namespace L4p.Common.Wcf
{
    public class WcfAsyncHost<T> : IWcfHost<T>
        where T : class
    {
        #region config

        public class Config
        {
            public int MaxRetries { get; set; }
            public TimeSpan RetrySpan { get; set; }

            public Config()
            {
                MaxRetries = 100;
                RetrySpan = 5.Seconds();
            }
        }

        #endregion

        #region members

        private readonly ILogFile _log;
        private readonly Config _config;
        private readonly IWcfHost<T> _impl;
        private readonly Task _task;
        private readonly CancellationTokenSource _cancellation;

        private string _uri;
        private Binding _binding;

        #endregion

        #region construction

        public static IWcfHost<T> New(ILogFile log, IWcfHost<T> impl)
        {
            var config = new Config();

            return
                new WcfAsyncHost<T>(log, config, impl);
        }

        public static IWcfHost New(ILogFile log, Config config, IWcfHost<T> impl)
        {
            return
                new WcfAsyncHost<T>(log, config, impl);
        }

        private WcfAsyncHost(ILogFile log, Config config, IWcfHost<T> impl)
        {
            _log = log;
            _config = config;
            _impl = impl;
            _cancellation = new CancellationTokenSource();

            var token = _cancellation.Token;
            _task = new Task(() => start_with_retries(token), token);
        }

        #endregion

        #region private

        private bool start_host(int no)
        {
            try
            {
                _impl.StartAt(_uri, _binding);
                return true;
            }
            catch (Exception ex)
            {
                _log.Trace("An attempt ({0}) to start a host failed; {1}", no, ex.Message);
                return false;
            }
        }

        private void start_with_retries(CancellationToken token)
        {
            var count = 0;

            while (true)
            {
                if (token.IsCancellationRequested)
                    break;

                if (start_host(count))
                    break;

                if (count++ > _config.MaxRetries)
                {
                    _log.Warn("Failed to start host, max retries is reached ({0}); ({1})", count, _uri);
                    break;
                }

                Thread.Sleep(_config.RetrySpan);

                _log.Trace("Trying to start host (retry={0}); ({1})", count, _uri);
            }
        }

        #endregion

        #region interface

        void IDisposable.Dispose()
        {
            _cancellation.Cancel();
            _impl.Dispose();
        }

        void IWcfHost.StartAt(string uri, Binding binding)
        {
            _uri = uri;
            _binding = binding;
            _task.Start();
        }

        void IWcfHost.Close()
        {
            _cancellation.Cancel();
            _impl.Close();
        }

        T IWcfHost<T>.Target
        {
            get { return _impl.Target; }
            set { _impl.Target = value; }
        }

        #endregion
    }
}