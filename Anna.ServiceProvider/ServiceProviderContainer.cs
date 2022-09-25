using Anna.DomainModel;
using Anna.DomainModel.Config;
using Anna.DomainModel.ObjectLifetimeChecker;
using Anna.Gui;
using Anna.Gui.ViewModels.ShortcutKey;
using Anna.Gui.Views.Dialogs.Base;
using Anna.Repository;
using Anna.UseCase;
using SimpleInjector;

namespace Anna.ServiceProvider;

public class ServiceProviderContainer : Container
{
    public ServiceProviderContainer(string logOutputDir, string appConfigFilePath)
    {
        RegisterSingleton<IObjectLifetimeCheckerUseCase,
#if DEBUG
            DefaultObjectLifetimeChecker
#else
            NopObjectLifetimeChecker
#endif
        >();

        var keyConfigFilePath = Path.Combine(Path.GetDirectoryName(appConfigFilePath) ?? "", "KeyConfig.json");

        RegisterSingleton(() =>
            new AppConfig(GetInstance<IObjectSerializerUseCase>()) { FilePath = appConfigFilePath });
        RegisterSingleton(() =>
            new KeyConfig(GetInstance<IObjectSerializerUseCase>()) { FilePath = keyConfigFilePath });
        RegisterSingleton<ILoggerUseCase>(() => new Log.Logger(logOutputDir));
        RegisterSingleton<IObjectSerializerUseCase, FileSystemObjectSerializer>();
        RegisterSingleton<App>();
        RegisterSingleton<ResourcesHolder>();
        RegisterSingleton<DomainModelOperator>();
        RegisterSingleton<ShortcutKeyManager>();

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
    }

    public void Destroy()
    {
        GetInstance<KeyConfig>().Save();
        GetInstance<AppConfig>().Save();

        var checker = GetInstance<IObjectLifetimeCheckerUseCase>();

        Dispose();

        checker.End();
        _logger.End("Application");
        _logger.Destroy();
    }

    private readonly ILoggerUseCase _logger;
}