using System.Net.Mime;
using System.Text;

namespace SimpleApi.Http;

public class HttpResponse
{
    private HttpResponse(byte[] responseBytes)
    {
        this.ResponseBytes = responseBytes;
    }

    public byte[] ResponseBytes { get; set; }
    
    private static byte[] GetBytes(string response)
    {
        return Encoding.UTF8.GetBytes(response);
    }
    
    public static HttpResponse GetResponse(int responseCode, string content, string contentType = MediaTypeNames.Text.Html)
    {
        return new(GetBytes($"""
                             {GetResponseHeaders(responseCode, contentType)}
                             
                             {content}
                             """));
    }

    public static HttpResponse GetResponse(int responseCode, byte[] bytes, string contentType = MediaTypeNames.Text.Html)
    {
        var responseBytes = GetBytes($"""
                              {GetResponseHeaders(responseCode, contentType)}

                              
                              """).ToList();
        responseBytes.AddRange(bytes);
        return new(responseBytes.ToArray());
    }

    private static string GetResponseHeaders(int responseCode, string contentType)
    {
        var headers = new StringBuilder();
        headers.Append($"HTTP/1.1 {responseCode} {GetStatusMessage(responseCode)}\n");
        headers.Append($"Date: {DateTime.UtcNow:R}\n");
        if (Server.Cors is not null) headers.Append(GetCorsHeaders());
        headers.Append("Server: assasinos_Simple_Web_Server\n");
        headers.Append($"Content-type: {contentType}; charset=UTF-8");
        return headers.ToString();
    } 
    private static string GetCorsHeaders()
    {
        var cors = Server.Cors;
        var headers = new StringBuilder();
        if (cors!.AllowOrigin is not null)
        {
            headers.Append($"Access-Control-Allow-Origin: {cors.AllowOrigin}\n");
        }
        if (cors.AllowMethods is not null)
        {
            headers.Append($"Access-Control-Allow-Methods: {cors.AllowMethods}\n");
        }
        if (cors.AllowHeaders is not null)
        {
            headers.Append($"Access-Control-Allow-Headers: {cors.AllowHeaders}\n");
        }
        if (cors.AllowCredentials is not null)
        {
            headers.Append($"Access-Control-Allow-Credentials: {cors.AllowCredentials}\n");
        }
        if (cors.MaxAge is not null)
        {
            headers.Append($"Access-Control-Max-Age: {cors.MaxAge}\n");
        }
        if (cors.ExposeHeaders is not null)
        {
            headers.Append($"Access-Control-Expose-Headers: {cors.ExposeHeaders}\n");
        }
        return headers.ToString().TrimEnd();
    }
    private static string GetStatusMessage(int responseCode)
    {
        return responseCode switch
        {
            200 => "OK",
            201 => "Created",
            202 => "Accepted",
            301 => "Moved Permanently",
            302 => "Found",
            303 => "See Other",
            307 => "Temporary Redirect",
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            405 => "Method Not Allowed",
            406 => "Not Acceptable",
            409 => "Conflict",
            410 => "Gone",
            422 => "Unprocessable Entity",
            429 => "Too Many Requests",
            500 => "Internal Server Error",
            502 => "Bad Gateway",
            503 => "Service Unavailable",
            504 => "Gateway Timeout",
            _ => "Unknown"
        };
    }
}