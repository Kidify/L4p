using System;
using System.Net;
using System.Web;

namespace L4p.WebApi
{
    public static class WebApiHelpers
    {
        private static readonly Type _controllerKey = typeof(IWebApiController);

        public static void SetApiController(this HttpContext http, IWebApiController controller)
        {
            http.Items[_controllerKey] = controller;
        }

        public static IWebApiController GetApiController(this HttpContext http)
        {
            return
                (IWebApiController) http.Items[_controllerKey];
        }

        public static RequestConfig GetRequestConfig(this HttpContext http)
        {
            var controller = http.GetApiController();
            var config = controller.GetRequestConfig(http);
            return config;
        }
    }
}