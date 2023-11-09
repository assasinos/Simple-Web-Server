using SimpleWebServer.Configuration;
using SimpleWebServer.Http.Headers.Response;

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
    
    public HttpServer Build()
    {
        return new HttpServer(Configuration);
    }

}