using System;
using System.IO;
using System.Text;
using System.Web;
using L4p.Common;
using L4p.Common.Extensions;
using L4p.Common.Loggers;


namespace L4p.WebApi
{
    public class ErrorReply
    {
        public string Error { get; set; }
    }

    public class HandleAllRequests : IHttpAsyncHandler
    {
        #region members

        private readonly ILogFile _log;

        #endregion

        #region construction

        public HandleAllRequests()
        {
            _log = LogFile.New("web_api_controller.log");
        }

        #endregion

        #region private

        private void set_common_headers(HttpContext http)
        {
            http.set_cors("*");
        }

        private void validate_request(HttpContext http, SingleRoute route)
        {
            // validate get/post request
        }

        private void process_request(HttpContext http, IAsyncOperation aop)
        {
            string path = http.Request.Path;

            var controller = http.GetApiController();
            var route = controller.GetRoute(path);

            if (route.HandlerIsSynchronous)
                aop.MarkAsSynchronous();

            validate_request(http, route);
            controller.ProcessRequest(http, route, aop.NotifyCompletion);
        }

        private void complete_failed_request(Exception ex, HttpContext http, IAsyncOperation aop)
        {
            var errMsg = "ProcessRequest() failed ({0}); {1}".Fmt(http.Request.Path, ex.Message);

            if (ex is BadRequestException)
                _log.Error(errMsg);
            else
                _log.Error(ex, errMsg);

            http.set_error_response(ex.Message);

            aop.MarkAsSynchronous();
            aop.NotifyCompletion();
        }

        #endregion

        #region interface

        void IHttpHandler.ProcessRequest(HttpContext http)
        {
            throw
                new ShouldNotBeCalledException();
        }

        bool IHttpHandler.IsReusable
        {
            get { return true; }
        }

        IAsyncResult IHttpAsyncHandler.BeginProcessRequest(HttpContext http, AsyncCallback cb, object extraData)
        {
            var aop = AsyncOperation.New(cb);
            set_common_headers(http);

            try
            {
                process_request(http, aop);
            }
            catch (Exception ex)
            {
                complete_failed_request(ex, http, aop);
            }

            return aop;
        }

        void IHttpAsyncHandler.EndProcessRequest(IAsyncResult result)
        {
        }

        #endregion
    }
}