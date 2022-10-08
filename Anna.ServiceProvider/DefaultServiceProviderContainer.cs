using Anna.DomainModel;
using Anna.DomainModel.Config;
using Anna.DomainModel.FileSystem;
using Anna.Gui;
using Anna.Gui.ShortcutKey;
using Anna.Gui.Views.Dialogs.Base;
using Anna.ObjectLifetimeChecker;
using Anna.Repository;
using Anna.UseCase;
using System.Runtime;

namespace Anna.ServiceProvider;

public class DefaultServiceProviderContainer : ServiceProviderContainerBase
{
    private DefaultServiceProviderContainer(string logOutputDir, string appConfigFilePath)
    {
        RegisterSingleton<IObjectLifetimeCheckerUseCase,
#if DEBUG
            DefaultObjectLifetimeChecker
#else
           NopObjectLifetimeChecker
#endif
        >();

        var configFolder = Path.GetDirectoryName(appConfigFilePath) ?? "";
        var keyConfigFilePath = Path.Combine(configFolder, KeyConfig.Filename);
        var jumpFolderConfigFilePath = Path.Combine(configFolder, JumpFolderConfig.Filename);

        RegisterSingleton(() =>
            new AppConfig(GetInstance<IObjectSerializerUseCase>()) { FilePath = appConfigFilePath });
        RegisterSingleton(() =>
            new KeyConfig(GetInstance<IObjectSerializerUseCase>()) { FilePath = keyConfigFilePath });
        RegisterSingleton(() =>
            new JumpFolderConfig(GetInstance<IObjectSerializerUseCase>()) { FilePath = jumpFolderConfigFilePath });
        RegisterSingleton<ILoggerUseCase>(() => new Log.DefaultLogger(logOutputDir));
        RegisterSingleton<IObjectSerializerUseCase, FileSystemObjectSerializer>();
        RegisterSingleton<IFolderServiceUseCase, FolderService>();
        RegisterSingleton<App>();
        RegisterSingleton<ResourcesHolder>();
        RegisterSingleton<DomainModelOperator>();
        RegisterSingleton<FolderPanelShortcutKey>();
        RegisterSingleton<TextViewerShortcutKey>();
        RegisterSingleton<ImageViewerShortcutKey>();

        // property injection
        RegisterInitializer<DialogBase>(d => d.Logger = GetInstance<ILoggerUseCase>());

        Options.ResolveUnregisteredConcreteTypes = true;

#if DEBUG
        Verify();
#endif

        _logger = GetInstance<ILoggerUseCase>();

        _logger.Start("Application");

        GetInstance<IObjectLifetimeCheckerUseCase>().Start(s => _logger.Error(s));
        GetInstance<AppConfig>().Load();
        GetInstance<KeyConfig>().Load();
        GetInstance<JumpFolderConfig>().Load();
    }

    public void Destroy()
    {
        GetInstance<JumpFolderConfig>().Save();
        GetInstance<KeyConfig>().Save();
        GetInstance<AppConfig>().Save();

        var checker = GetInstance<IObjectLifetimeCheckerUseCase>();

        Dispose();

        checker.End();
        _logger.End("Application");
        _logger.Destroy();
    }

    public static DefaultServiceProviderContainer Create(string[] args)
    {
        var commandLine = CommandLine.Parse(args);

        var appConfigFilePath = commandLine is null
            ? CommandLine.DefaultAppConfigFilePath
            : commandLine.AppConfigFilePath;

        // Assembly loading optimization
        var configDir = Path.GetDirectoryName(appConfigFilePath) ??
                        CommandLine.DefaultAppConfigFilePath;
        {
            Directory.CreateDirectory(configDir);
            ProfileOptimization.SetProfileRoot(configDir);
            ProfileOptimization.StartProfile("Startup.Profile");
        }

        return new DefaultServiceProviderContainer(configDir, appConfigFilePath);
    }

    private readonly ILoggerUseCase _logger;
}