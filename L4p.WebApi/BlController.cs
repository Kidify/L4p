using System;
using System.Web;
using L4p.Common.Extensions;
using L4p.Common.Helpers;
using L4p.Common.Json;
using L4p.Common.Loggers;

namespace L4p.WebApi
{
    public interface IBlController
    {
        void RegisterRoutes(IHttpServer server);
        void ProcessRequest(RouteHandler handler, HttpContext http);
    }

    public abstract class BlController : IBlController
    {
        private static readonly string _oops = "Oops, something went wrong ({0})";

        protected T parse_arguments<T>(HttpContext http, IHttpArguments args = null)
        {
            if (args == null)
                args = HttpArguments.New(http);

            var data = args.DataAsJson();

            try
            {
                return
                    data.Parse<T>();
            }
            catch (Exception ex)
            {
                throw 
                    new BadRequestException(ex, "Bad json in 'data' parameter; {0}", ex.Message);
            }
        }

        protected void fill_meta_data(HttpContext http, IAnyRequest request, IHttpArguments args = null)
        {
            if (args == null)
                args = HttpArguments.New(http);

            var meta = request.GetMeta;

            if (args.Has("en"))
                meta.Language = AnyRequest.ELanguage.En;
            else
            if (args.Has("ar"))
                meta.Language = AnyRequest.ELanguage.Ar;
            else
            if (args.Has("ru"))
                meta.Language = AnyRequest.ELanguage.Ru;
            else
                meta.Language = AnyRequest.ELanguage.En;
        }

        protected void start_request(HttpContext http, IAnyRequest request)
        {
            var inArgs = request.GetIn;

            if (inArgs.TrackingId.IsEmpty())
                inArgs.TrackingId = TrackingId.New();

            var outArgs = request.GetOut;

            outArgs.TrackingId = inArgs.TrackingId;
            outArgs.ErrorCode = AnyRequest.Ok;
            outArgs.ErrorMessage = String.Empty;

            http.set_item<IAnyRequest>(request);
        }

        protected void end_request(IAnyRequest request)
        {
            var inArgs = request.GetIn;
            var trackingId = inArgs.TrackingId;
            var outArgs = request.GetOut;

            if (outArgs != null)
            {
                outArgs.TrackingId = trackingId;

                if (outArgs.ErrorCode == AnyRequest.Ok)
                    return;

                Log.Error("Request with unexpected error code ({2}: {3}) trackingId='{0}' {1}", 
                    trackingId, inArgs.ToJson(), outArgs.ErrorCode, outArgs.ErrorMessage);

                if (outArgs.ErrorCode != "UxError")
                    outArgs.ErrorMessage = _oops.Fmt(trackingId);
            }
        }

        protected void end_failed_request(HttpContext http, Exception ex)
        {
            var request = http.get_item<IAnyRequest>(null);

            var inArgs = request != null ? request.GetIn : null;
            var trackingId = inArgs != null ? inArgs.TrackingId : "n/a";

            Log.Error(ex, "Unexpected exception while processing request with trackingId='{0}' {1}", trackingId, inArgs.ToJson());

            var errorMsg = ex is BadRequestException ?
                ex.Message :
                _oops.Fmt(trackingId);

            var outArgs = new AnyRequest.FailedArgs
            {
                TrackingId = trackingId,
                ErrorCode = AnyRequest.Failed,
                ErrorMessage = errorMsg
            };

            http.set_json_response(outArgs);
        }

        void IBlController.RegisterRoutes(IHttpServer server)
        {
            RegisterRoutes(server);
        }

        void IBlController.ProcessRequest(RouteHandler handler, HttpContext http)
        {
            try
            {
                handler(http);
            }
            catch (Exception ex)
            {
                end_failed_request(http, ex);
            }
        }

        protected abstract void RegisterRoutes(IHttpServer server);
        protected abstract ILogFile Log { get; }
    }
}