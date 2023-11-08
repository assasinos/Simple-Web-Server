﻿using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SimpleApi.Http;
using HttpMethod = SimpleApi.Http.HttpMethod;

namespace SimpleApi;

public class Server
{
    private readonly IPAddress _ipAddress;
    private readonly int _port;
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    private readonly Dictionary<string,(Func<HttpRequest, HttpResponse> handler, HttpMethod method)> _routes = new();
    
    public Server(IPAddress ipAddress, int port)
    {
        _ipAddress = ipAddress;
        _port = port;
    }

    public void RegisterRoute(string route, HttpMethod method , Func<HttpRequest, HttpResponse> handler)
    {
        if (!_routes.TryAdd(route.ToLower(), (handler, method)))
        {
            throw new Exception($"There was an error registering the route {route}");
        }
    }
    public void RegisterRoute(string route, HttpMethod method , string path)
    {
        if (!_routes.TryAdd(route.ToLower(), (new Func<HttpRequest, HttpResponse>(request => HttpResponse.GetResponse(200,File.ReadAllBytes(path))), method)))
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
    
            
            var route = string.Empty;
            
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
            var tcpListener = new TcpListener(_ipAddress, _port);

            tcpListener.Start();



            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                // Buffer for reading data
                var bytes = new byte[1024];
                var data = string.Empty;
                Debug.WriteLine("Waiting for a Connection...");

                var socket = await tcpListener.AcceptSocketAsync(_cancellationTokenSource.Token);
                Debug.WriteLine($"Connection from {socket.RemoteEndPoint}");
                socket.Receive(bytes);
                data = Encoding.UTF8.GetString(bytes);
                Debug.WriteLine($"Received:\n {data}\n");
                
                
                if (!HttpRequest.TryParse(data,null, out var request))
                {
                    await socket.SendHttpResponse(HttpResponse.GetResponse(400, "Malformed Response"), _cancellationTokenSource.Token);
                    continue;
                }
                
                //404
                if (!_routes.TryGetValue(request.Path.ToLower(), out var handler))
                {
                    await socket.SendHttpResponse(HttpResponse.GetResponse(404, ""), _cancellationTokenSource.Token);
                    continue;
                }
                
                //405
                if (handler.method != request.Method)
                {
                    await socket.SendHttpResponse(HttpResponse.GetResponse(405, ""), _cancellationTokenSource.Token);
                    continue;
                }

                //500 
                //If something goes wrong with the handler
                //TODO: ADD logger
                if (handler.handler is null)
                {
                    await socket.SendHttpResponse(HttpResponse.GetResponse(500, "There was an error processing your request"), _cancellationTokenSource.Token);
                    continue;
                }
                
                
                try
                {
                    await socket.SendHttpResponse(handler.handler(request), _cancellationTokenSource.Token);
                }
                catch (Exception e)
                {
                    await socket.SendHttpResponse(HttpResponse.GetResponse(500, "There was an error processing your request"), _cancellationTokenSource.Token);
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