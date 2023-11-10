namespace SimpleWebServer.Logger;

public class Logger
{
    private readonly StreamWriter _streamWriter;

    public Logger(StreamWriter streamWriter)
    {
        _streamWriter = streamWriter;
        _streamWriter.AutoFlush = true;
    }

    //Add dependency injection
    private void Log(string message, LogLevel logLevel = LogLevel.Info)
    {
        _streamWriter.WriteLine($"[{DateTime.Now}] [{logLevel}] {message}");
    }
    public void LogWarning(string message) => Log(message, LogLevel.Warning);
    public void LogError(string message) => Log(message, LogLevel.Error);
    public void LogDebug(string message) => Log(message, LogLevel.Debug);
    public void LogInfo(string message) => Log(message, LogLevel.Info);
    

}