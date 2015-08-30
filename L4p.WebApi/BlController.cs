using System;
using System.Collections.Generic;
using System.Web;
using L4p.Common.Extensions;
using L4p.Common.Helpers;
using L4p.Common.Json;
using L4p.Common.Loggers;

namespace L4p.WebApi
{
    public interface IBlController
    {
        string Name { get; }
        void Initialize(IHttpServer server);
        void ProcessRequest(RouteHandler handler, HttpContext http);
        void Shut();
    }

    public class BlBaseController
    {
        public string Name { get { return GetType().Name; } }
        public void Shut() {}
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
                meta.Language = ELanguage.En;
            else
            if (args.Has("ar"))
                meta.Language = ELanguage.Ar;
            else
            if (args.Has("ru"))
                meta.Language = ELanguage.Ru;
            else
            if (args.Has("fr"))
                meta.Language = ELanguage.Fr;
            else
            if (args.Has("de"))
                meta.Language = ELanguage.De;
            else
                meta.Language = ELanguage.En;
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

                Log.Warn("Response ({0}) with unexpected error code ({1})': '{2}'; {3}", 
                    trackingId, outArgs.ErrorCode, outArgs.ErrorMessage, inArgs.AsSingleLineJson());

                var feInputFailure =
                    outArgs.ErrorCode.StartsWith("RC_") && outArgs.ErrorCode != "RC_EXCEPTION";

                if (feInputFailure)
                    return;

                if (outArgs.ErrorCode != "UxError")
                    outArgs.ErrorMessage = _oops.Fmt(trackingId);
            }
        }

        protected void end_failed_request(HttpContext http, Exception ex)
        {
            var request = http.get_item<IAnyRequest>(null);

            var inArgs = request != null ? request.GetIn : null;
            var trackingId = inArgs != null ? inArgs.TrackingId : "n/a";

            var path = http.Request.Path;
            var queryStr = http.Request.QueryString;

            var isBadInput = ex is BadRequestException;

            if (isBadInput)
            {
                Log.Warn("{4} while processing request '{0}/{1}' (trackingId='{2}'); '{3}'",
                    path, queryStr, trackingId, inArgs.AsSingleLineJson(), ex.Message);

            }
            else
            {
                Log.Error(ex, "Unexpected exception while processing request '{0}/{1}' (trackingId='{2}'); '{3}'",
                    path, queryStr, trackingId, inArgs.AsSingleLineJson());

            }

            var errorMsg = isBadInput ? ex.Message : _oops.Fmt(trackingId);

            var outArgs = new AnyRequest.FailedArgs
            {
                TrackingId = trackingId,
                ErrorCode = AnyRequest.Failed,
                ErrorMessage = errorMsg
            };

            http.set_jsonp_response(outArgs);
        }

        protected bool validate_request(IAnyRequest request, Action validateIt)
        {
            try
            {
                validateIt();
                return true;
            }
            catch (Exception ex)
            {
                var rOut = request.GetOut;

                rOut.ErrorCode = "BAD_INPUT";
                rOut.ErrorMessage = ex.Message;

                Log.Warn("Bad request ({0}): {1}; {2}", request.GetIn.TrackingId, ex.Message, request.GetIn.AsSingleLineJson());

                return false;
            }
        }

        #region interface

        string IBlController.Name { get { return Name; } }

        void IBlController.Initialize(IHttpServer server)
        {
            Initialize(server);
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

        void IBlController.Shut()
        {
            Shut();
        }

        protected virtual string Name
        {
            get { return GetType().Name; }
        }

        protected abstract void Initialize(IHttpServer server);
        protected abstract ILogFile Log { get; }

        protected virtual void Start() {}
        protected virtual void Shut() {}

        #endregion
    }
}