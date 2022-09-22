using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using System.Diagnostics;
using ILogger=Anna.DomainModel.Interfaces.ILogger;

namespace Anna.Log;

public class Logger : ILogger
{
    public Logger(string logOutputDir)
    {
        // https://qiita.com/soi/items/e74918a924c02e3a3097

        const string template =
            "| {Timestamp:HH:mm:ss.fff} | {Level:u4} | {ThreadId:00}:{ThreadName} | {ProcessId:00}:{ProcessName} | {Message:j} | {AssemblyName} | {MemoryUsage} B|{NewLine}{Exception}";

        var logFilePath = Path.Combine(logOutputDir, "logs/log.txt");
        
        _logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.WithThreadId()
            .Enrich.WithThreadName().Enrich.WithProperty("ThreadName", "__")
            .Enrich.WithProcessId().Enrich.WithProcessName()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentUserName()
            .Enrich.WithAssemblyName()
            .Enrich.WithAssemblyVersion()
            .Enrich.WithMemoryUsage()
            .Enrich.WithExceptionDetails()
            .MinimumLevel.Verbose()
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

    private readonly Serilog.Core.Logger _logger;
}