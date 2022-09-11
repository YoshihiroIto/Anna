using Anna.DomainModel;
using Anna.DomainModel.Interface;
using Anna.DomainModel.Operator;
using Anna.Interactor;

namespace Anna.ServiceProvider;

public class Container : SimpleInjector.Container
{
    public Container()
    {
        RegisterSingleton<IObjectLifetimeChecker,
#if DEBUG
            DefaultObjectLifetimeChecker
#else
            NopObjectLifetimeChecker
#endif
        >();

        RegisterSingleton<App>();
        RegisterSingleton<IDomainModelOperator, DomainModelOperator>();

        Options.ResolveUnregisteredConcreteTypes = true;

#if DEBUG
        Verify();
#endif
    }
}