
namespace SimpleApi.Http;

public class HttpRequest : IParsable<HttpRequest>
{
    public HttpMethod Method { get; set; }
    public string Path { get; set; }
    public string? UserAgent { get; set; }
    public string? Accept { get; set; }
    public string? Host { get; set; }
    public string? AcceptEncoding { get; set; }
    public string? Connection { get; set; }
    public int ContentLength { get; set; }
    public string? ContentType { get; set; }
    public string? Body { get; set; }

    private HttpRequest(string methodString, string path)
    {
        
        Method = Enum.TryParse<HttpMethod>(methodString, out var method) ? method : throw new Exception("Invalid Method");
        Path = path;
    }
    public HttpRequest(HttpMethod method , string path)
    {
        
        Method = method;
        Path = path;
    }
    
    public static HttpRequest Parse(string s, IFormatProvider? provider)
    {
        var lines = s.Split(Environment.NewLine);
        var requestLine = lines[0].Split(' ');
        var request = new HttpRequest(requestLine[0], requestLine[1]);
        
        var properties = typeof(HttpRequest).GetProperties();
        
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
            property?.SetValue(request, Convert.ChangeType(headerValue, property.PropertyType, provider));
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