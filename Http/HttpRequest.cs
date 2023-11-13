using SimpleWebServer.Http.Headers.Request;

namespace SimpleWebServer.Http;

public class HttpRequest : IParsable<HttpRequest>
{
    public HttpRequestHeaders Headers { get; set; } = new();
    public string Body { get; set; } = string.Empty;

    private HttpRequest(string methodString, string path)
    {
        
        Headers.Method = Enum.TryParse<HttpMethod>(methodString, out var method) ? method : throw new Exception("Invalid Method");
        Headers.Path = path;
    }
    public HttpRequest(HttpMethod method , string path)
    {
        
        Headers.Method = method;
        Headers.Path = path;
    }
    
    public static HttpRequest Parse(string s, IFormatProvider? provider)
    {
        var lines = s.Split(Environment.NewLine);
        var requestLine = lines[0].Split(' ');
        var request = new HttpRequest(requestLine[0], requestLine[1]);
        
        var properties = typeof(HttpRequestHeaders).GetProperties();
        
        for (var i = 1; i < lines.Length; i++)
        {
            var line = lines[i];
            // Empty line is the end of the headers
            if (string.IsNullOrWhiteSpace(line))
            {
                request.Body = string.Join(Environment.NewLine, lines.Skip(i + 1));
                // Remove trailing null characters
                request.Body = request.Body.TrimEnd((Char)0);
                break;
            }
            var header = line.Split(':');
            var headerName = header[0].Replace("-", "");
            var headerValue = header[1];
            var property = properties.FirstOrDefault(p => p.Name.Equals(headerName, StringComparison.OrdinalIgnoreCase));
            property?.SetValue(request.Headers, Convert.ChangeType(headerValue, property.PropertyType, provider));
        }

        return request;
    }

    public static bool TryParse(string? s, IFormatProvider? provider, out HttpRequest result)
    {
        try
        {
            if (s is null) throw new ArgumentNullException(nameof(s));
            result = Parse(s, provider);
            return true;
        }
        catch
        {
            result = null!;
            return false;
        }
    }
}