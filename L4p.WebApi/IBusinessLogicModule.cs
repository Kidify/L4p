using System;
using System.Web;

namespace L4p.WebApi
{
    public interface IBusinessLogicModule : IBlController
    {
        string Name { get; }

        void Initialize(IHttpServer server);
        void Shut();
    }
}