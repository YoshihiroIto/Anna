﻿using Anna.DomainModel;
using Anna.DomainModel.Interface;
using Anna.DomainModel.Operator;
using Anna.Interactor;
using Anna.ViewModels.ShortcutKey;
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
        RegisterSingleton<IDomainModelOperator, DomainModelOperator>();
        RegisterSingleton<ShortcutKeyManager>();

        Options.ResolveUnregisteredConcreteTypes = true;

#if DEBUG
        Verify();
#endif
        
        _logger = GetInstance<ILogger>();
        
        _logger.Information("Start");

        GetInstance<IObjectLifetimeChecker>().Start(s => _logger.Error(s));
    }

    public new void Dispose()
    {
        var checker = GetInstance<IObjectLifetimeChecker>();

        base.Dispose();

        checker.End();
        _logger.Information("End");
    }

    private readonly ILogger _logger;
}