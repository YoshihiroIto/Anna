using Anna.Log;
using Anna.ObjectLifetimeChecker;
using Anna.ServiceProvider;
using Anna.UseCase;

namespace Anna.TestFoundation;

public class TestServiceProvider : ServiceProviderBase
{
    public TestServiceProvider()
    {
        RegisterSingleton<IObjectLifetimeCheckerUseCase, NopObjectLifetimeChecker>();
        RegisterSingleton<ILoggerUseCase, NopLogger>();

        Options.ResolveUnregisteredConcreteTypes = true;

#if DEBUG
        Verify();
#endif
    }
}