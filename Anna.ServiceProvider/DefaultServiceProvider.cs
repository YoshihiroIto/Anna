using Anna.DomainModel;
using Anna.DomainModel.Config;
using Anna.DomainModel.Service;
using Anna.Gui;
using Anna.Gui.Views.Windows.Base;
using Anna.Service.Compressor;
using Anna.Service.Logger;
using Anna.Service.ObjectLifetimeChecker;
using Anna.Service.Repository;
using Anna.Service.Services;
using Anna.Service.Workers;
using SimpleInjector;
using SimpleInjector.Diagnostics;
using System.Runtime;

namespace Anna.ServiceProvider;

public sealed class DefaultServiceProvider : ServiceProviderBase
{
    private readonly ILoggerService _logger;

    private DefaultServiceProvider(string logOutputDir, string appConfigFilePath)
    {
        Register(logOutputDir, appConfigFilePath);

        _logger = GetInstance<ILoggerService>();
        _logger.Start("Application");

        GetInstance<IObjectLifetimeCheckerService>().Start(s => _logger.Error(s));
        GetInstance<AppConfig>().Load();
        GetInstance<KeyConfig>().Load();
        GetInstance<JumpFolderConfig>().Load();
    }

    public void Destroy()
    {
        GetInstance<JumpFolderConfig>().Save();
        GetInstance<KeyConfig>().Save();
        GetInstance<AppConfig>().Save();

        var checker = GetInstance<IObjectLifetimeCheckerService>();

        Dispose();

        checker.End();
        _logger.End("Application");
        _logger.Destroy();
    }

    public static DefaultServiceProvider Create(string[] args)
    {
        var commandLine = CommandLine.Parse(args);

        var appConfigFilePath = commandLine is null
            ? CommandLine.DefaultAppConfigFilePath
            : commandLine.AppConfigFilePath;

        var configDir = Path.GetDirectoryName(appConfigFilePath) ?? CommandLine.DefaultAppConfigFilePath;
        {
            // Assembly loading optimization
            Directory.CreateDirectory(configDir);
            ProfileOptimization.SetProfileRoot(configDir);
            ProfileOptimization.StartProfile("Startup.Profile");
        }

        return new DefaultServiceProvider(configDir, appConfigFilePath);
    }

    private void Register(string logOutputDir, string appConfigFilePath)
    {
        var configFolder = Path.GetDirectoryName(appConfigFilePath) ?? "";
        var keyConfigFilePath = Path.Combine(configFolder, KeyConfig.FileName);
        var jumpFolderConfigFilePath = Path.Combine(configFolder, JumpFolderConfig.FileName);

        RegisterSingleton(() => new AppConfig(this) { FilePath = appConfigFilePath });
        RegisterSingleton(() => new KeyConfig(this) { FilePath = keyConfigFilePath });
        RegisterSingleton(() => new JumpFolderConfig(this) { FilePath = jumpFolderConfigFilePath });
        RegisterSingleton<App>();
        RegisterSingleton<ResourcesHolder>();
        RegisterSingleton<DomainModelOperator>();

        RegisterSingleton<ILoggerService>(() => new DefaultLogger(logOutputDir));
        RegisterSingleton<IObjectSerializerService, FileSystemObjectSerializer>();
        RegisterSingleton<IFileSystemIsAccessibleService, FileSystemIsAccessibleService>();
        RegisterSingleton<IFolderHistoryService, FolderHistoryService>();
        RegisterSingleton<ICompressorService, CompressorService>();

#if DEBUG
        RegisterSingleton<IObjectLifetimeCheckerService, DefaultObjectLifetimeChecker>();
#else
        RegisterSingleton<IObjectLifetimeCheckerService, NopObjectLifetimeChecker>();
#endif

        if (OperatingSystem.IsWindows())
        {
            RegisterSingleton<ITrashCanService, Service.Windows.TrashCanService>();
            RegisterSingleton<IDefaultValueService, Service.Windows.DefaultValueService>();
        }
        else if (OperatingSystem.IsMacOS())
        {
            RegisterSingleton<ITrashCanService, Service.MacOS.TrashCanService>();
            RegisterSingleton<IDefaultValueService, Service.MacOS.DefaultValueService>();
        }
        else if (OperatingSystem.IsLinux())
        {
            RegisterSingleton<ITrashCanService, Service.Linux.TrashCanService>();
            RegisterSingleton<IDefaultValueService, Service.Linux.DefaultValueService>();
        }

        // transient
        Register<IBackgroundWorker, BackgroundWorker>(Lifestyle.Transient);

        // property injection
        RegisterInitializer<WindowBase>(d => d.Logger = GetInstance<ILoggerService>());

        GetRegistration(typeof(IBackgroundWorker))!.Registration
            .SuppressDiagnosticWarning(DiagnosticType.DisposableTransientComponent, "dispose manually.");

        Options.ResolveUnregisteredConcreteTypes = true;

#if DEBUG
        Verify();
#endif
    }
}