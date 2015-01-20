using System;
using System.IO;
using System.Web;

namespace L4p.WebApi
{
    public class SingleRoute
    {
        public string Path { get; set; }
        public IBlController Controller { get; set; }
        public RouteHandler Handler { get; set; }
        public AsyncRouteHandler AsyncHandler { get; set; }

        internal bool IsPostRequest { get; set; }
        internal bool IsGetRequest { get; set; }

        internal ASplit Split { get; set; }

        public bool HandlerIsSynchronous
        {
            get { return Handler != null; }
        }

        public bool HandlerIsAsynchronous
        {
            get { return AsyncHandler != null; }
        }
    }
}