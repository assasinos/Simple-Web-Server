namespace SimpleApi.Http.Headers.Request;

public class HttpRequestHeaders
{
    public string Path { get; set; }
    public string? UserAgent { get; set; }
    public string? Accept { get; set; }
    public string? Host { get; set; }
    public string? AcceptEncoding { get; set; }
    public string? Connection { get; set; }
    public int ContentLength { get; set; }
    public string? ContentType { get; set; }
    public string? Origin { get; set; }
    public string? Body { get; set; }
    public HttpMethod Method { get; set; }

}