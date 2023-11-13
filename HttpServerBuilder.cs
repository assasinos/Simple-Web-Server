using SimpleWebServer.Configuration;
using SimpleWebServer.Http.Headers.Response;
using SimpleWebServer.Services;

namespace SimpleWebServer;

public class HttpServerBuilder
{
    private readonly ServerConfiguration Configuration;

    public HttpServerBuilder(ServerConfiguration configuration)
    {
        Configuration = configuration;
    }
    public void UseCors(Cors cors)
    {
        Configuration.Cors = cors;
    }
    
    public void RegisterService(IService service)
    {
        Configuration.Services.Add(service);
    }
    
    public HttpServer Build()
    {
        return new HttpServer(Configuration);
    }

}