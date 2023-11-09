using System.Net;
using SimpleApi.Http.Headers.Response;

namespace SimpleApi.Configuration;

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
    
    public void UseCors(Cors cors)
    {
        Cors = cors;
    }
    
    
}