using Anna.UseCase;

namespace Anna.Log;

public class NopLogger : ILoggerUseCase
{
    public void Destroy() {}
    public void Verbose(string message) {}
    public void Debug(string message) {}
    public void Information(string message) {}
    public void Warning(string message) {}
    public void Error(string message) {}
    public void Fatal(string message) {}
}