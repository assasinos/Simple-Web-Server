namespace SimpleWebServer.Http.Headers.Response;

public class Cors
{
    public string? AllowOrigin { get; init; }
    public string? AllowMethods { get; init; }
    public string? AllowHeaders { get; init; }
    public string? AllowCredentials { get; init; }
    public string? MaxAge { get; init; }
    public string? ExposeHeaders { get; init; }


}