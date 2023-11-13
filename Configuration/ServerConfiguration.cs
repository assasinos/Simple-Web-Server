using System.Net;
using SimpleWebServer.Http.Headers.Response;
using SimpleWebServer.Services;

namespace SimpleWebServer.Configuration;

public class ServerConfiguration
{
    internal readonly IPAddress IpAddress;
    internal readonly int Port;
    internal Cors? Cors = null ;
    internal readonly List<IService> Services = new();
    
    public ServerConfiguration(IPAddress ipAddress, int port)
    {
        IpAddress = ipAddress;
        Port = port;
    }
    
    
}