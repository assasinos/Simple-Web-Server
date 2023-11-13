namespace SimpleWebServer.Services.Logging;

public class Logger : IService
{
    private readonly StreamWriter _streamWriter;
    
    public Logger()
    {
        _streamWriter = new StreamWriter(Console.OpenStandardOutput());
        _streamWriter.AutoFlush = true;
    }

    //Add dependency injection
    private void Log(string message, LogLevel logLevel = LogLevel.Info)
    {
        _streamWriter.Write($"[{DateTime.Now}]");
        Console.ForegroundColor = logLevel switch
        {
            LogLevel.Info => ConsoleColor.Cyan,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Debug => ConsoleColor.Blue,
            _ => ConsoleColor.White
        };
        _streamWriter.Write($" [{logLevel}] ");
        Console.ResetColor();
        _streamWriter.WriteLine(message);
    }
    public void LogWarning(string message) => Log(message, LogLevel.Warning);
    public void LogError(string message) => Log(message, LogLevel.Error);

    //Only log debug messages in debug mode
    public void LogDebug(string message)
    {
#if DEBUG
        Log(message, LogLevel.Debug);
#endif
    }
    public void LogInfo(string message) => Log(message, LogLevel.Info);
    

}