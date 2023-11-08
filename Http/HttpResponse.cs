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
                             HTTP/1.1 {responseCode} {GetStatusMessage(responseCode)}
                             Date: {DateTime.UtcNow:R}
                             Server: assasinos_Simple_Web_Server
                             Content-type: {contentType}; charset=UTF-8

                             {content}
                             """));
    }
    public static HttpResponse GetResponse(int responseCode, byte[] bytes, string contentType = MediaTypeNames.Text.Html)
    {
        var responseBytes = GetBytes($"""
                              HTTP/1.1 {responseCode} {GetStatusMessage(responseCode)}
                              Date: {DateTime.UtcNow:R}
                              Server: assasinos_Simple_Web_Server
                              Content-type: {contentType}; charset=UTF-8

                              
                              """).ToList();
        responseBytes.AddRange(bytes);
        return new(responseBytes.ToArray());
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