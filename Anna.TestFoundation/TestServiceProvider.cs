using Anna.Service.Services;
using Anna.Service.Workers;
using Anna.ServiceProvider;
using Moq;

namespace Anna.TestFoundation;

public sealed class TestServiceProvider : ServiceProviderBase
{
    public TestServiceProvider()
    {
        RegisterSingleton(Mock.Of<IObjectLifetimeCheckerService>);
        RegisterSingleton(Mock.Of<ILogService>);
        RegisterSingleton(Mock.Of<IBackgroundWorker>);
        RegisterSingleton(Mock.Of<IFolderHistoryService>);

        Options.ResolveUnregisteredConcreteTypes = true;

#if DEBUG
        Verify();
#endif
    }
}
