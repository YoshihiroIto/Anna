using Anna.Log;
using Anna.ObjectLifetimeChecker;
using Anna.UseCase;
using SimpleInjector;

namespace Anna.TestFoundation;

public class TestServiceProviderContainer : Container, IServiceProviderContainer
{
    public TestServiceProviderContainer()
    {
        RegisterSingleton<IServiceProviderContainer>(() => this);
        RegisterSingleton<IObjectLifetimeCheckerUseCase, NopObjectLifetimeChecker>();
        RegisterSingleton<ILoggerUseCase, NopLogger>();

        Options.ResolveUnregisteredConcreteTypes = true;
        
#if DEBUG
        Verify();
#endif
    }
}