using System.Net;
using SimpleWebServer.Http.Headers.Response;

namespace SimpleWebServer.Configuration;

public class ServerConfiguration
{
    internal readonly IPAddress IpAddress;
    internal readonly int Port;
    internal Cors? Cors = null ;
    
    public ServerConfiguration(IPAddress ipAddress, int port)
    {
        IpAddress = ipAddress;
        Port = port;
    }
    
    
}