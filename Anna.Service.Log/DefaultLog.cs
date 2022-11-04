using Anna.Service.Services;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using System.Diagnostics;

namespace Anna.Service.Log;

public sealed class DefaultLog : ILogService
{
    private readonly Serilog.Core.Logger _logger;
    
    public DefaultLog(string logOutputDir)
    {
        // https://qiita.com/soi/items/e74918a924c02e3a3097

        const string template =
            "| {Timestamp:HH:mm:ss.fff} | {Level:u4} | {ThreadId:00}:{ThreadName} | {ProcessId:00}:{ProcessName} | {Message:j} | {AssemblyName} | {MemoryUsage} B | {NewLine}{Exception}";

        var logFilePath = Path.Combine(logOutputDir, "logs/log.txt");

        _logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.WithThreadId()
            .Enrich.WithThreadName()
            .Enrich.WithProperty("ThreadName", "__")
            .Enrich.WithProcessId()
            .Enrich.WithProcessName()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentUserName()
            .Enrich.WithAssemblyName()
            .Enrich.WithAssemblyVersion()
            .Enrich.WithMemoryUsage()
            .Enrich.WithExceptionDetails()
            .WriteTo.Console(outputTemplate: template)
            .WriteTo.Debug(outputTemplate: template)
            .WriteTo.File(logFilePath,
                LogEventLevel.Verbose,
                outputTemplate: template,
                rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }

    public void Destroy()
    {
        _logger.Dispose();
    }

    // ReSharper disable TemplateIsNotCompileTimeConstantProblem
    public void Verbose(string message) => _logger.Verbose(message);
    public void Debug(string message) => _logger.Debug(message);
    public void Information(string message) => _logger.Information(message);
    public void Warning(string message) => _logger.Warning(message);

    public void Error(string message)
    {
        _logger.Error(message);
        Debugger.Break();
    }

    public void Fatal(string message)
    {
        _logger.Fatal(message);
        Debugger.Break();
    }
    // ReSharper restore TemplateIsNotCompileTimeConstantProblem
}