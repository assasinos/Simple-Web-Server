using System.Net.Mime;
using System.Text;

namespace SimpleApi.Http;

public class HttpResponse
{
    public HttpResponse(byte[] responseBytes)
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
                             Server: assasinos_Simple_API_Server
                             Content-type: {contentType}; charset=UTF-8

                             {content}
                             """));
    }
    

    private static string GetStatusMessage(int responseCode)
    {
        return responseCode switch
        {
            200 => "OK",
            400 => "Bad Request",
            401 => "Unauthorized",
            404 => "Not Found",
            405 => "Method Not Allowed",
            500 => "Internal Server Error",
            _ => "Unknown"
        };
    }
}