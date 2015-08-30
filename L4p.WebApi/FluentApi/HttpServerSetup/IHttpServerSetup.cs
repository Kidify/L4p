namespace L4p.WebApi.FluentApi.HttpServerSetup
{
    public interface IGetRequest
    {
        IMapRoute Route(string route);
    }

    public interface IPostRequest
    {
        IMapRoute Route(string route);
    }

    public interface IRequestSpecifier
    {
        IPostRequest AsPost();
        IGetRequest AsGet();
        IGetRequest AsGetJsonP();
        IGetRequest AsGetPng();
        IMapRoute Route(string route);
    }

    public interface IMapRoute
    {
        IRequestSpecifier MapTo(IBlController listener, RouteHandler handler);
    }

    public interface IHttpServerSetup
    {
        IMapRoute Route(string route);
    }
}