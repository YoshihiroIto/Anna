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

        Serilog.Log.Logger = new LoggerConfiguration()
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

    // ReSharper disable TemplateIsNotCompileTimeConstantProblem
    public void Verbose(string message) => Serilog.Log.Verbose(message);
    public void Debug(string message) => Serilog.Log.Debug(message);
    public void Information(string message) => Serilog.Log.Information(message);
    public void Warning(string message) => Serilog.Log.Warning(message);
    
    public void Error(string message)
    {
        Serilog.Log.Error(message);
        Debugger.Break();
    }
    
    public void Fatal(string message)
    {
        Serilog.Log.Fatal(message);
        Debugger.Break();
    }
    // ReSharper restore TemplateIsNotCompileTimeConstantProblem
}