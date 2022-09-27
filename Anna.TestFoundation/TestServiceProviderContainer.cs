using Anna.Log;
using Anna.ObjectLifetimeChecker;
using Anna.ServiceProvider;
using Anna.UseCase;

namespace Anna.TestFoundation;

public class TestServiceProviderContainer : ServiceProviderContainerBase
{
    public TestServiceProviderContainer()
    {
        RegisterSingleton<IObjectLifetimeCheckerUseCase, NopObjectLifetimeChecker>();
        RegisterSingleton<ILoggerUseCase, NopLogger>();

        Options.ResolveUnregisteredConcreteTypes = true;

#if DEBUG
        Verify();
#endif
    }
}