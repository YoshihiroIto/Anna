﻿using Anna.DomainModel;
using Anna.DomainModel.Config;
using Anna.DomainModel.Service;
using Anna.Gui;
using Anna.Gui.Views.Windows.Base;
using Anna.Service.Log;
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
    private readonly ILogService _log;

    private DefaultServiceProvider(string logOutputDir, string appConfigFilePath)
    {
        Register(logOutputDir, appConfigFilePath);

        _log = GetInstance<ILogService>();
        _log.Start("Application");

        GetInstance<IObjectLifetimeCheckerService>().Start(s => _log.Error(s));
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
        _log.End("Application");
        _log.Destroy();
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
        RegisterSingleton<IObjectLifetimeCheckerService,
#if DEBUG
            DefaultObjectLifetimeChecker
#else
            NopObjectLifetimeChecker
#endif
        >();

        var configFolder = Path.GetDirectoryName(appConfigFilePath) ?? "";
        var keyConfigFilePath = Path.Combine(configFolder, KeyConfig.FileName);
        var jumpFolderConfigFilePath = Path.Combine(configFolder, JumpFolderConfig.FileName);

        RegisterSingleton(() =>
            new AppConfig(GetInstance<IObjectSerializerService>()) { FilePath = appConfigFilePath });
        RegisterSingleton(() =>
            new KeyConfig(GetInstance<IObjectSerializerService>()) { FilePath = keyConfigFilePath });
        RegisterSingleton(() =>
            new JumpFolderConfig(GetInstance<IObjectSerializerService>()) { FilePath = jumpFolderConfigFilePath });
        RegisterSingleton<ILogService>(() => new DefaultLog(logOutputDir));
        RegisterSingleton<IObjectSerializerService, FileSystemObjectSerializer>();
        RegisterSingleton<IFileSystemIsAccessibleService, FileSystemIsAccessibleService>();
        RegisterSingleton<IFolderHistoryService, FolderHistoryService>();
        RegisterSingleton<App>();
        RegisterSingleton<ResourcesHolder>();
        RegisterSingleton<DomainModelOperator>();
        RegisterSingleton<ITrashCanService>(() =>
        {
            if (OperatingSystem.IsWindows())
                return GetInstance<Service.Windows.TrashCanService>();

            if (OperatingSystem.IsMacOS())
                return GetInstance<Service.MacOS.TrashCanService>();

            if (OperatingSystem.IsLinux())
                return GetInstance<Service.Linux.TrashCanService>();

            throw new PlatformNotSupportedException();
        });

        Register<IBackgroundWorker, BackgroundWorker>(Lifestyle.Transient);

        // property injection
        RegisterInitializer<WindowBase>(d => d.Log = GetInstance<ILogService>());

        GetRegistration(typeof(IBackgroundWorker))!.Registration
            .SuppressDiagnosticWarning(DiagnosticType.DisposableTransientComponent,
                "dispose manually.");

        Options.ResolveUnregisteredConcreteTypes = true;

#if DEBUG
        Verify();
#endif
    }
}