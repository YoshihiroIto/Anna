using Anna.DomainModel.Interface;
using Anna.Interactor.Foundation;

namespace Anna.ServiceProvider;

public class Container : SimpleInjector.Container
{
    public Container()
    {
        RegisterSingleton<IObjectLifetimeChecker,
#if DEBUG
            DefaultObjectLifetimeChecker
#else
            NopLifeCycleChecker
#endif
        >();

        Options.ResolveUnregisteredConcreteTypes = true;

#if DEBUG
        Verify();
#endif
    }
}