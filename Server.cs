using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SimpleApi.Configuration;
using SimpleApi.Http;
using SimpleApi.Http.Headers.Response;
using HttpMethod = SimpleApi.Http.HttpMethod;

namespace SimpleApi;

public class Server
{

    public static ServerConfiguration Configuration;
    
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    
    private readonly Dictionary<string,(Func<HttpRequest, HttpResponse> handler, HttpMethod method)> _routes = new();
    
    public Server(IPAddress ipAddress, int port, Cors? cors = null)
    {
        Configuration = new ServerConfiguration(ipAddress, port);
    }

    public void RegisterRoute(string route, HttpMethod method , Func<HttpRequest, HttpResponse> handler)
    {
        if (!_routes.TryAdd(route.ToLower(), (handler, method)))
        {
            throw new Exception($"There was an error registering the route {route}");
        }
    }
    
    /// <summary>
    /// Registers new route
    /// </summary>
    /// <param name="route">Route to resource</param>
    /// <param name="method">HTTP method</param>
    /// <param name="path">Path to file</param>
    /// <exception cref="Exception">Throws an exception if route couldn't be registered</exception>
    public void RegisterRoute(string route, HttpMethod method , string path)
    {
        if (!_routes.TryAdd(route.ToLower(), ((_ => HttpResponse.GetResponse(200,File.ReadAllBytes(path))), method)))
        {
            throw new Exception($"There was an error registering the route {route}");
        }
    }

    public void RegisterDirectory(string baseRoute = "/" , HttpMethod method = HttpMethod.GET, string path = "pages")
    {
        var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var baseFilePath = file.Replace(path, "").Replace("\\","/").Replace(".html", "");


            string route;
            
            //Check for index.html
            if (Path.GetFileName(file) == "index.html")
            {
                route = Path.Combine(baseRoute, baseFilePath.Replace("/index",""));
                
                RegisterRoute(route, method, file);
                continue;
            }

            route = Path.Combine(baseRoute, baseFilePath);
            
            
            RegisterRoute(route, method, file);
        }
    }
    
    
    
    public void Start()
    {
        Task.Run(async () =>
        {
            var tcpListener = new TcpListener(Configuration.IpAddress, Configuration.Port);

            tcpListener.Start();



            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                // Buffer for reading data
                var bytes = new byte[1024];
                Debug.WriteLine("Waiting for a Connection...");

                var socket = await tcpListener.AcceptSocketAsync(_cancellationTokenSource.Token);
                Debug.WriteLine($"Connection from {socket.RemoteEndPoint}");
                socket.Receive(bytes);
                var data = Encoding.UTF8.GetString(bytes);
                Debug.WriteLine($"Received:\n {data}\n");
                
                
                if (!HttpRequest.TryParse(data,null, out var request))
                {
                    await socket.SendHttpResponse(HttpResponse.GetResponse(400, "Malformed Response"), _cancellationTokenSource.Token);
                    continue;
                }
                
                //404
                if (!_routes.TryGetValue(request.Headers.Path.ToLower(), out var handler))
                {
                    await socket.SendHttpResponse(HttpResponse.GetResponse(404, ""), _cancellationTokenSource.Token);
                    continue;
                }
                
                //405
                if (handler.method != request.Headers.Method)
                {
                    await socket.SendHttpResponse(HttpResponse.GetResponse(405, ""), _cancellationTokenSource.Token);
                    continue;
                }

                
                //TODO: ADD logger
                try
                {
                    await socket.SendHttpResponse(handler.handler(request), _cancellationTokenSource.Token);
                }
                catch (Exception e)
                {
                    #if DEBUG
                    await socket.SendHttpResponse(HttpResponse.GetResponse(500, $"There was an error processing your request\nError:\n{e}"), _cancellationTokenSource.Token);
                    #else
                    await socket.SendHttpResponse(HttpResponse.GetResponse(500, "There was an error processing your request"), _cancellationTokenSource.Token);
                    #endif
                    
                    continue;
                }
                
                await Task.Delay(1000, _cancellationTokenSource.Token);
            }

            tcpListener.Stop();
        });
    }


    public void Stop()
    {
        _cancellationTokenSource.Cancel();
    }
}