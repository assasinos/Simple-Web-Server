
using System.Net.Mime;

namespace SimpleWebServer.Http.Mime;

public static class Mime
{
    public static string GetContentType(string fileExtension)
    {
        return fileExtension switch
        {
            
            ".css" => "text/css",
            ".js" => "text/javascript",
            ".json" => MediaTypeNames.Application.Json,

            #region Images
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            ".tiff" => "image/tiff",
            ".tif" => "image/tiff",
            ".ico" => "image/x-icon",
            #endregion
            
            #region Videos
            ".webm" => "video/webm",
            #endregion
            
            ".html" => MediaTypeNames.Text.Html,
            _ => MediaTypeNames.Text.Plain
        };
    }
}