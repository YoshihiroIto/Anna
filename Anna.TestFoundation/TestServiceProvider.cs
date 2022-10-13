using Anna.Log;
using Anna.ObjectLifetimeChecker;
using Anna.Service;
using Anna.Service.Interfaces;
using Anna.ServiceProvider;
using System.ComponentModel;

namespace Anna.TestFoundation;

public sealed class TestServiceProvider : ServiceProviderBase
{
    public TestServiceProvider()
    {
        RegisterSingleton<IObjectLifetimeCheckerService, NopObjectLifetimeChecker>();
        RegisterSingleton<ILoggerService, NopLogger>();
        RegisterSingleton<IBackgroundService, MockBackgroundService>();

        Options.ResolveUnregisteredConcreteTypes = true;

#if DEBUG
        Verify();
#endif
    }
}

internal class MockBackgroundService : IBackgroundService
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public bool IsInProcessing => false;
    public double ProgressRatio => 0;
    public string Message => "";
    public void CopyFileSystemEntry(string destPath, IEnumerable<IEntry> sourceEntries)
    {
        throw new NotImplementedException();
    }
}
