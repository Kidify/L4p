using System;
using System.Diagnostics;
using L4p.Common;
using L4p.Common.Extensions;

namespace L4p.WebApi
{
    public static class WebFactory
    {
        private static void initialize_module(ModuleEntryPoint entryPoint, IWebApiController controller)
        {
            try
            {
                entryPoint(controller);
            }
            catch (Exception ex)
            {
                string msg = "Failed to initialize module '{0}'".Fmt(entryPoint.Method.Name);
                TraceLogger.WriteLine(ex.FormatHierarchy(msg, true, true));
            }
        }

        public static IWebApiController NewController(
            MakeRequestConfigHandler makeRequestConfig, ModuleEntryPoint[] modulesEntryPoints)
        {
            var controller = WebApiController.New(makeRequestConfig);

            foreach (var entryPoint in modulesEntryPoints)
            {
                initialize_module(entryPoint, controller);
            }

            return controller;
        }
    }
}