using Anna.DomainModel.Interface;
using Anna.Interactor.Foundations;

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

        Options.ResolveUnregisteredConcreteTypes = true;

#if DEBUG
        Verify();
#endif
    }
}