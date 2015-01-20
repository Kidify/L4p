using L4p.WebApi.FluentApi.HttpServerSetup;

namespace L4p.WebApi.FluentApi
{
    public static class FluentExtentions
    {
        public static IHttpServerSetup Setup(this IHttpServer server)
        {
            return
                HttpServerSetter.New(server);
        }
    }
}