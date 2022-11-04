using Anna.Service.Services;

namespace Anna.Service.Logger;

public sealed class NopLogger : ILoggerService
{
    public void Destroy() {}
    public void Verbose(string message) {}
    public void Debug(string message) {}
    public void Information(string message) {}
    public void Warning(string message) {}
    public void Error(string message) {}
    public void Fatal(string message) {}
}