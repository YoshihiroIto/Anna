using Anna.DomainModel;
using Anna.DomainModel.ObjectLifetimeChecker;
using Anna.Gui.ViewModels.ShortcutKey;
using Anna.Gui.Views.Dialogs.Base;
using Anna.Repository;
using Anna.Strings;
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

        RegisterSingleton(() => new Config(GetInstance<IObjectSerializerUseCase>()) { FilePath = appConfigFilePath });
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
        GetInstance<Config>().LoadAsync().AsTask().Wait();
    }

    public void Destroy()
    {
        GetInstance<Config>().SaveAsync().AsTask().Wait();

        var checker = GetInstance<IObjectLifetimeCheckerUseCase>();

        Dispose();

        checker.End();
        _logger.End("Application");
        _logger.Destroy();
    }

    private readonly ILoggerUseCase _logger;
}