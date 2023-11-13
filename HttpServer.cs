using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using SimpleWebServer.Configuration;
using SimpleWebServer.Http;
using SimpleWebServer.Http.Mime;
using SimpleWebServer.Services;
using SimpleWebServer.Services.Logging;
using SimpleWebServer.Validation;
using HttpMethod = SimpleWebServer.Http.HttpMethod;

namespace SimpleWebServer;

public class HttpServer
{
    internal static ServerConfiguration Configuration = null!;

    //Debug logger  
    public static readonly Logger Logger = new Logger();

    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    
    private readonly Dictionary<string, (Delegate handler, HttpMethod method)> _methods = new();


    internal HttpServer(ServerConfiguration configuration)
    {
        Configuration = configuration;
    }
    
    public void RegisterRoute(string route, HttpMethod method, Delegate handler)
    {
        //Check if delegate is correct
        if (!RouteHandlerValidator.ValidateRouteHandler(handler))
        {
            throw new Exception($"Route {route} has an invalid handler");
        }
        if (!_methods.TryAdd(route.ToLower(), (handler, method)))
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
    public void RegisterRoute(string route, HttpMethod method, string path)
    {
        if (!_methods.TryAdd(route.ToLower(),(()=> HttpResponse.GetResponse(200,File.ReadAllBytes(path),Mime.GetContentType(Path.GetExtension(path))),method)))
        {
            throw new Exception($"There was an error registering the route {route}");
        }
    }

    public void RegisterDirectory(string baseRoute = "/", HttpMethod method = HttpMethod.GET, string path = "pages")
    {
        var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var baseFilePath = file.Replace(path, "").Replace("\\", "/").Replace(".html", "");


            string route;

            //Check for index.html
            if (Path.GetFileName(file).ToLower() == "index.html")
            {
                route = Path.Combine(baseRoute, baseFilePath.Replace("/index", ""));
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
                Logger.LogDebug($"Connection accepted from {socket.RemoteEndPoint}");
                socket.Receive(bytes);
                var data = Encoding.UTF8.GetString(bytes);
                Logger.LogDebug($"Received:\n {data}\n");


                if (!HttpRequest.TryParse(data, null, out var request))
                {
                    await socket.SendHttpResponse(HttpResponse.GetResponse(400, "Malformed Response"),
                        _cancellationTokenSource.Token);
                    continue;
                }

                //404
                if (!_methods.TryGetValue(request.Headers.Path.ToLower(), out var handler))
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


                
                try
                {
                    var parameters = handler.handler.Method.GetParameters();
                    var args = new object[parameters.Length];
                    foreach (var parameterInfo in parameters)
                    {
                        //Check if parameter is HttpRequest
                        if (parameterInfo.ParameterType.IsAssignableFrom(typeof(HttpRequest)))
                        {
                            args[parameterInfo.Position] = request;
                            continue;
                        }
                        
                        //Check if _services contains parameter
                        var service = Configuration.Services.FirstOrDefault(x => x.GetType() == parameterInfo.ParameterType);
                        if (service is not null)
                        {
                            args[parameterInfo.Position] = service;
                            continue;
                        }
                        //TODO: Add Support for parameters from request (FromBody,FromQuery etc.)
                        
                    }
                    
                    var response = (HttpResponse) handler.handler.DynamicInvoke(args)!;
                    await socket.SendHttpResponse(response, _cancellationTokenSource.Token);
                }
                catch (Exception e)
                {
                    Logger.LogError($"There was an error processing request: {e}");
#if DEBUG
                    await socket.SendHttpResponse(
                        HttpResponse.GetResponse(500, $"There was an error processing your request\nError:\n{e}"),
                        _cancellationTokenSource.Token);
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