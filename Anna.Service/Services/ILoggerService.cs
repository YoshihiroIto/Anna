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
    
    public void Start(string name) => Information($"Start [{name}]");
    public void End(string name) => Information($"End   [{name}]");
}