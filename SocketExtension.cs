using System.Diagnostics;
using System.Net.Sockets;
using SimpleWebServer.Http;

namespace SimpleWebServer;

public static class SocketExtension
{

    private static Task TimeoutTask = Task.Delay(TimeSpan.FromSeconds(1));
    public static async Task SendHttpResponse(this Socket socket, HttpResponse response, CancellationToken cancellationToken)
    {
        try
        {

            //Check if the socket is still connected
            if (!socket.Poll(1, SelectMode.SelectWrite))
            {
                return;
            }

            await socket.SendAsync(response.ResponseBytes);

            //Read bytes send back from the client
            byte[] buffer = new byte[1024];
            int bytesRead;
            do
            {
                var dataReceived = await Task.WhenAny(socket.ReceiveAsync(buffer, SocketFlags.None), TimeoutTask);

                if (dataReceived == TimeoutTask)
                {

                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                    break;
                }

                bytesRead = ((Task<int>)dataReceived).Result;
            } while (bytesRead > 0);


        }
        catch (Exception e)
        {
            //TODO: ADD logger
            //Maybe change this in the future
            Debug.WriteLine($"There was an error processing request\nError:\n{e}");
            return;
        }
        

    }


}