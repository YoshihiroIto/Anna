namespace Anna.Service.Services;

public interface ILoggerService
{
    public void Destroy();

    void Verbose(string message);
    void Debug(string message);
    void Information(string message);
    void Warning(string message);
    void Error(string message);
    void Fatal(string message);
}

public static class LoggerExtensions
{
    public static void Start(this ILoggerService logger, string name) => logger.Information($"Start [{name}]");

    public static void End(this ILoggerService logger, string name) => logger.Information($"End   [{name}]");
}