using Anna.Log;
using Anna.ObjectLifetimeChecker;
using Anna.Service;
using Anna.ServiceProvider;

namespace Anna.TestFoundation;

public class TestServiceProvider : ServiceProviderBase
{
    public TestServiceProvider()
    {
        RegisterSingleton<IObjectLifetimeCheckerService, NopObjectLifetimeChecker>();
        RegisterSingleton<ILoggerService, NopLogger>();

        Options.ResolveUnregisteredConcreteTypes = true;

#if DEBUG
        Verify();
#endif
    }
}