using Anna.DomainModel;
using Anna.DomainModel.Interface;
using Anna.DomainModel.Operator;
using Anna.Interactor;
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
        RegisterSingleton<App>();
        RegisterSingleton<IDomainModelOperator, DomainModelOperator>();

        Options.ResolveUnregisteredConcreteTypes = true;

#if DEBUG
        Verify();
#endif
    }
}