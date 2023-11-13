using SimpleWebServer.Http;

namespace SimpleWebServer.Validation;

public class RouteHandlerValidator
{
    internal static bool ValidateRouteHandler(Delegate handler)
    {
        //Return type must be HttpResponse
        return handler.Method.ReturnType.IsAssignableTo(typeof(HttpResponse));
    }
}