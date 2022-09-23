using Anna.DomainModel;
using Anna.DomainModel.Interfaces;
using Anna.DomainModel.ObjectLifetimeChecker;
using Anna.Gui.Interactors;
using Anna.Gui.ViewModels.ShortcutKey;
using Anna.Gui.Views.Dialogs.Base;
using Anna.Repository;
using Anna.UseCase;
using Anna.UseCase.Interfaces;
using SimpleInjector;

namespace Anna.ServiceProvider;

public class ServiceProviderContainer : Container
{
    public ServiceProviderContainer(string logOutputDir, string appConfigFilePath)
    {
        RegisterSingleton<IObjectLifetimeChecker,
#if DEBUG
            DefaultObjectLifetimeChecker
#else
            NopObjectLifetimeChecker
#endif
        >();

        RegisterSingleton(() => new Config { FilePath = appConfigFilePath });
        RegisterSingleton<ILogger>(() => new Log.Logger(logOutputDir));
        RegisterSingleton<App>();
        RegisterSingleton<DomainModelOperator>();
        RegisterSingleton<ShortcutKeyManager>();
        RegisterSingleton<IDialogOperator, DialogOperator>();
        
        // repository
        RegisterSingleton<IObjectReader, FileSystemObjectReader>();
        RegisterSingleton<IObjectWriter, FileSystemObjectWriter>();

        // property injection
        RegisterInitializer<DialogBase>(d => d.Logger = GetInstance<ILogger>() );

        Options.ResolveUnregisteredConcreteTypes = true;

#if DEBUG
        Verify();
#endif

        _logger = GetInstance<ILogger>();

        _logger.Start("Application");

        GetInstance<IObjectLifetimeChecker>().Start(s => _logger.Error(s));
    }

    public void Destroy()
    {
        var checker = GetInstance<IObjectLifetimeChecker>();

        Dispose();

        checker.End();
        _logger.End("Application");
        _logger.Destroy();
    }

    private readonly ILogger _logger;
}