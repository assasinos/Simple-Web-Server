using System.Net.Sockets;
using SimpleWebServer.Http;

namespace SimpleWebServer;

public static class SocketExtension
{

    private static Task TimeoutTask = Task.Delay(TimeSpan.FromSeconds(1));
    public static async Task SendHttpResponse(this Socket socket, HttpResponse response, CancellationToken cancellationToken)
    {
        
        await socket.SendAsync(response.ResponseBytes);

        //Read bytes send back from the client
        byte[] buffer = new byte[1024];
        int bytesRead;
        do
        {
            var dataReceived = await Task.WhenAny( socket.ReceiveAsync(buffer, SocketFlags.None),TimeoutTask );
            
            if (dataReceived == TimeoutTask)
            {
                        
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                break;
            }
            bytesRead = ((Task<int>)dataReceived).Result;
        } while (bytesRead > 0);
        
        
        

    }


}