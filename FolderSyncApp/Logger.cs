using Serilog;

namespace FolderSyncApp;

public class Logger : Interfaces.ILogger
{
    private readonly ILogger _logger;

    public Logger(string logFile)
    {
        _logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(logFile)
            .CreateLogger();
    }

    public void Log(string message)
    {
        _logger.Information(message);
    }
}