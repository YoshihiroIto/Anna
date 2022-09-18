namespace Anna.DomainModel.Interfaces;

public interface ILogger
{
    void Verbose(string message);
    void Debug(string message);
    void Information(string message);
    void Warning(string message);
    void Error(string message);
    void Fatal(string message);
}